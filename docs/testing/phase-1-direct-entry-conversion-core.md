# Test Runbook: Phase 1 — Direct Entry Conversion Core

> Status: DRAFT
> Version: v2
> Last updated: 2026-08-14
> Covers: F-001–F-008, F-014 (see [docs/project-management.md](../project-management.md))
> Scenario reference: [docs/test-cases.md](../test-cases.md) TC-001–TC-032
> Source data: [docs/stash/Direct Entry - File Specification CommBiz.md](../stash/Direct%20Entry%20-%20File%20Specification%20CommBiz.md) §5 (Sample File)

This runbook is a step-by-step manual verification guide for the full Direct Entry conversion
pipeline: **Host (Kestrel) → Wolverine dispatch → Payment Type Router (F-004) → Validator (F-005) →
Detail/Header/Self-balancing/Trailer mapping (F-006/F-007/F-014) → assembled response (F-008)**. Each
section can be run independently against a locally running instance of the API.

> **Contract note (v2):** this runbook was rewritten to match the actual current request/response
> shape. The request body is a **plain JSON array** of payment instructions in Shaw and Partners'
> native payload shape (`paymentTypeCode`, `accountNo`, `sourceBank*`, `paymentDate`, `amount`,
> `createBy`) — there is no top-level `fileName`/`instructions` wrapper, and per-instruction fields
> like `indicator`, `transactionCode`, `accountTitle`, `lodgementReference`, `traceBsb`,
> `traceAccountNumber` and `remitterName` no longer exist on the request; those are now sourced from
> the `DirectEntry` configuration section (organisation-level, one value applies to every instruction
> in the batch). The endpoint is `POST /convert` (not `/direct-entry/convert`), and there is no
> `/diagnostics/ping` endpoint — it was removed once a real business endpoint existed to smoke-test
> Wolverine dispatch against.

## ⚠️ Known caveat: rejection responses return HTTP 200, not 4xx

Every example below that describes a "rejected batch" still returns **`200 OK`** at the transport
level — the request reached the endpoint and was handled without a routing/server error. Rejection is
signalled only in the JSON body via `"success": false` plus a populated `errors` array. This is a known
open product-decision item, not a bug in this runbook or in the implementation it describes. Treat every
`200 OK` + `"success": false` response below as the expected, correct result for that scenario.

## Prerequisites

1. Start the host from the repo root:
   ```powershell
   dotnet run --project src/CommBiz.Api
   ```
2. Confirm Kestrel is listening on `http://localhost:5182` (per
   [src/CommBiz.Api/Properties/launchSettings.json](../../src/CommBiz.Api/Properties/launchSettings.json), `http` profile).
3. All examples below use PowerShell's `Invoke-RestMethod`. A `curl.exe` equivalent is noted where
   useful. Ready-to-run equivalents for every scenario below also live in
   [tests/smoke/requests.http](../../tests/smoke/requests.http).
4. The `DirectEntry` configuration section (`src/CommBiz.Api/appsettings.json`) supplies every
   organisation-level field used in mapping — `TraceAccountBsb`/`TraceAccountAccNo` (settlement
   account), `TransactionCode` (the direction applied to every real detail record), `Title`,
   `LodgementReferenceDetails`, `NameOfRemitter`, `AmountOfWithholdingTax`, etc. Examples below assume
   the checked-in `appsettings.json` values (`TransactionCode: "13"`, `TraceAccountBsb: "062-000"`,
   `TraceAccountAccNo: "21120227"`).

---

## F-001 — Host scaffold smoke test

```powershell
Invoke-RestMethod -Uri http://localhost:5182/health -Method Get
```

**Expected:** `200 OK`, body `{"status":"Healthy"}`.

---

## F-002/F-003 — Wolverine dispatch + happy path: single-instruction batch (baseline) — TC-001, TC-002, TC-028

There is no standalone diagnostics endpoint any more — `POST /convert` is itself the smoke test for
Wolverine dispatch (`Program.cs` routes it through `IMessageBus.InvokeAsync`, not directly). The
sample data below is drawn from [tests/smoke/requests.http](../../tests/smoke/requests.http) and
mirrors a real two-instruction batch. Because F-014 dropped the minimum batch size to 1, a
single-instruction batch (TC-028) is also a valid happy-path case in its own right and is shown first.

```powershell
$body = @'
[
  {
    "paymentTypeCode": "DE",
    "accountNo": "S1605677",
    "paymentSourceTypeCode": "CMA",
    "sourceBankAccountName": "SOPHIA CLARK",
    "sourceBankAccountNo": "111375004",
    "sourceBankBSB": "015141",
    "paymentDate": "2026-08-20T10:00:00",
    "sourceCurrency": "AUD",
    "sourceAmount": 0.0,
    "amount": 7500.0,
    "createBy": "James Harris"
  }
]
'@

Invoke-RestMethod -Uri http://localhost:5182/convert -Method Post `
  -ContentType 'application/json' -Body $body
```

**Expected:** `200 OK`, JSON envelope shaped as:
```json
{
  "success": true,
  "convertedText": "<header>\r\n<detail>\r\n<self-balancing detail>\r\n<trailer>\r\n",
  "errors": null
}
```
Per ADR-008, `convertedText` is the full converted file inline — no download link. Note the request
body is a **plain JSON array**, not an object with a top-level `fileName`/`instructions` wrapper —
every field on it is per-instruction; there is no header-level payload data (the header record's
static fields all come from `DirectEntry` configuration, and its date comes from the earliest
instruction's `paymentDate` — see F-007 below).

Splitting `convertedText` on `\r\n` for this single-instruction batch yields exactly **4** records:
1 header + 1 real detail + 1 self-balancing detail (F-014) + 1 trailer. See F-006/F-014/F-007 below for
field-level verification of each.

---

## F-003 — Happy path: multi-instruction batch — TC-003

Two-instruction variant of the baseline above (also in
[tests/smoke/requests.http](../../tests/smoke/requests.http)):

```powershell
$body = @'
[
  {
    "paymentTypeCode": "DE",
    "accountNo": "S1605677",
    "paymentSourceTypeCode": "CMA",
    "sourceBankAccountName": "SOPHIA CLARK",
    "sourceBankAccountNo": "111375004",
    "sourceBankBSB": "015141",
    "paymentDate": "2026-08-20T10:00:00",
    "sourceCurrency": "AUD",
    "sourceAmount": 0.0,
    "amount": 7500.0,
    "createBy": "James Harris"
  },
  {
    "paymentTypeCode": "DE",
    "accountNo": "S1605678",
    "paymentSourceTypeCode": "CMA",
    "sourceBankAccountName": "LIAM NGUYEN",
    "sourceBankAccountNo": "222486115",
    "sourceBankBSB": "063111",
    "paymentDate": "2026-08-20T10:00:00",
    "sourceCurrency": "AUD",
    "sourceAmount": 0.0,
    "amount": 12500.0,
    "createBy": "James Harris"
  }
]
'@

Invoke-RestMethod -Uri http://localhost:5182/convert -Method Post `
  -ContentType 'application/json' -Body $body
```

**Expected:** `200 OK`, `success: true`; splitting `convertedText` on `\r\n` yields **5** records:
1 header + 2 real details + 1 self-balancing detail + 1 trailer. This request/response pair is the
baseline for every field-level check in F-006/F-007/F-014 below.

---

## F-004 — Payment Type Router: unsupported payment type rejects the whole batch — TC-004, TC-005

Take the F-003 body and change the **first** instruction's `paymentTypeCode` to `"BPAY"` (anything
other than `"DE"`, case-insensitively):

```powershell
# ...same body as F-003, but instruction 0's paymentTypeCode = "BPAY"
Invoke-RestMethod -Uri http://localhost:5182/convert -Method Post `
  -ContentType 'application/json' -Body $body
```

**Expected:** `200 OK`,
```json
{
  "success": false,
  "convertedText": null,
  "errors": [
    { "index": 0, "reason": "Unsupported payment type 'BPAY'." }
  ]
}
```
Note the entire batch is rejected — the second (otherwise valid) instruction is **not** partially
converted. `index` is the 0-based position of the offending instruction in the request array. If
every instruction has an unsupported type (TC-005), every index is reported.

Router check runs **before** field validation (F-005) — an unsupported-type instruction with an
otherwise-invalid `sourceBankBSB` will still only report the payment-type error, not both.

---

## F-005 — Field validation: one scenario per rule — TC-006–TC-012

For each row, start from the F-003 happy-path body and apply only the described mutation to
**one field on one instruction**, then POST. All of these return `200 OK` with `"success": false`,
`"convertedText": null`, and an `errors` array containing an entry whose `reason` matches the pattern
shown.

| Rule | Mutation | Expected `errors[].reason` (substring) | `index` |
|---|---|---|---|
| `accountNo` blank | `instructions[0].accountNo = ""` | `AccountNo must not be blank` | `0` |
| `sourceBankBSB` malformed (hyphen) | `instructions[0].sourceBankBSB = "015-141"` | `SourceBankBsb '015-141' must be exactly 6 numeric digits` | `0` |
| `sourceBankBSB` too short | `instructions[0].sourceBankBSB = "01514"` | `SourceBankBsb` ... `must be exactly 6 numeric digits` | `0` |
| `sourceBankBSB` too long | `instructions[0].sourceBankBSB = "0151411"` | `SourceBankBsb` ... `must be exactly 6 numeric digits` | `0` |
| `sourceBankBSB` non-numeric | `instructions[0].sourceBankBSB = "01514A"` | `SourceBankBsb` ... `must be exactly 6 numeric digits` | `0` |
| `sourceBankAccountNo` too long (10 chars) | `instructions[0].sourceBankAccountNo = "1234567890"` | `SourceBankAccountNo` ... `is invalid` | `0` |
| `sourceBankAccountNo` disallowed character | `instructions[0].sourceBankAccountNo = "12345$678"` | `SourceBankAccountNo` ... `is invalid` | `0` |
| `sourceBankAccountNo` all-zero | `instructions[0].sourceBankAccountNo = "000000000"` | `SourceBankAccountNo` ... `is invalid` | `0` |
| `sourceBankAccountNo` all-blank | `instructions[0].sourceBankAccountNo = "   "` | `SourceBankAccountNo` ... `is invalid` | `0` |
| `amount` zero | `instructions[0].amount = 0` | `Amount '0' must be positive and convert to at most 10 digits of cents` | `0` |
| `amount` negative | `instructions[0].amount = -1` | `Amount` ... `must be positive` | `0` |
| `amount` over 10 digits of cents | `instructions[0].amount = 100000000.00` | `Amount` ... `at most 10 digits of cents` | `0` |
| Minimum batch size (F-014) | remove all instructions (`[]`) | `Payment file must contain at least 1 payment instruction(s) (found 0)` | `-1` |

**One-invalid-among-many (regression-sensitive):** apply only the `sourceBankBSB`-format mutation to
`instructions[0]` in a batch that otherwise has 5+ valid instructions. Expected: the whole batch is
still rejected with exactly one error entry (`index: 0`) — the 5+ remaining valid instructions must
not appear anywhere in the response, and must not be partially converted.

There is no request-level equivalent any more of the old `indicator`, `transactionCode`,
`accountTitle`, `lodgementReference`, `traceBsb`/`traceAccountNumber`, `remitterName`, or
`withholdingTaxAmountInCents` field rules — those values are no longer part of the request payload
(see the "Contract note" at the top of this document), so there is nothing per-instruction to
validate for them.

---

## F-014 — Self-balancing (contra) detail record + minimum-1 batch size — TC-028–TC-032

### TC-028 — Single-instruction batch converts successfully (regression-sensitive)

Already demonstrated above in F-002/F-003 — before F-014, a 1-instruction batch was rejected under
the old minimum-2 rule. It now succeeds because the self-balancing record supplies the second
required detail record structurally.

### TC-029 — Zero-instruction batch is still rejected

```powershell
Invoke-RestMethod -Uri http://localhost:5182/convert -Method Post `
  -ContentType 'application/json' -Body '[]'
```

**Expected:** `200 OK`,
```json
{
  "success": false,
  "convertedText": null,
  "errors": [
    { "index": -1, "reason": "Payment file must contain at least 1 payment instruction(s) (found 0)." }
  ]
}
```
Note the wording change from the pre-F-014 runbook: the reason now reads **"at least 1"**, not
"at least 2".

### TC-030 — Self-balancing detail record field positions

Using any successful response's `convertedText`, the record **immediately before the trailer** is the
self-balancing (contra) record. It is 120 characters, same layout as an ordinary detail record, but
every field except Amount comes from `DirectEntry` configuration rather than any instruction:

| Field | Position | Width | Expected value |
|---|---|---|---|
| Record Type | 1 | 1 | `1` |
| BSB Number | 2–8 | 7 | `settings.TraceAccountBsb` (settlement account, e.g. `062-000`) |
| Account Number | 9–17 | 9 | `settings.TraceAccountAccNo`, right-justified, space-padded (e.g. `21120227` → ` 21120227`) |
| Indicator | 18 | 1 | `N` |
| Transaction Code | 19–20 | 2 | inverse of `settings.TransactionCode` — `50` when configured code is `13`, or `13` when configured code is `50` |
| Amount (cents, zero-filled) | 21–30 | 10 | sum of every real instruction's `amount`, converted to cents (round-then-sum — see TC-032 below) |
| Title of Account | 31–62 | 32 | `settings.Title` |
| Lodgement Reference | 63–80 | 18 | `settings.LodgementReferenceDetails` |
| Trace BSB | 81–87 | 7 | `settings.TraceAccountBsb` |
| Trace Account Number | 88–96 | 9 | `settings.TraceAccountAccNo`, right-justified, space-padded |
| Remitter Name | 97–112 | 16 | `settings.NameOfRemitter` |
| Withholding Tax Amount | 113–120 | 8 | `settings.AmountOfWithholdingTax` (always `00000000`) |

Verify with PowerShell (checked-in `appsettings.json` values, F-003 two-instruction batch: 7500.00 +
12500.00 = 20000.00 → 2000000 cents):
```powershell
$lines = $text -split "`r`n" | Where-Object { $_ -ne "" }
$selfBalancing = $lines[-2]   # last real line before the trailer
$selfBalancing.Substring(1,7)    # "062-000"  (TraceAccountBsb)
$selfBalancing.Substring(8,9).Trim()  # "21120227" (TraceAccountAccNo)
$selfBalancing.Substring(18,2)   # "50" (inverse of TransactionCode "13")
$selfBalancing.Substring(20,10)  # "0002000000"
```

### TC-031 — Self-balancing record is positioned immediately before the trailer (regression-sensitive)

Convert a batch of 3+ valid instructions. Expected line order: header, one detail record per
instruction (in request order), then the self-balancing detail record, then the trailer — never
after the trailer, never interleaved with the real detail records.

```powershell
$lines = $text -split "`r`n" | Where-Object { $_ -ne "" }
$lines | ForEach-Object { $_.Substring(0,1) }   # e.g. 0,1,1,1,1,7 for a 4-instruction batch
```
The second-to-last record type `1` line is always the self-balancing record; the last is always the
trailer (`7`).

### TC-032 — Trailer reconciles to zero net once the self-balancing record is included (regression-sensitive)

See F-007 below for the full trailer field table. Key invariant: because the self-balancing record
always posts the batch total against the inverse of the configured `TransactionCode`, File Credit
Total always equals File Debit Total (both equal the sum of the real instructions' amounts in cents),
so File Net Total is always `0000000000`, regardless of how many real instructions are in the batch or
their individual amounts.

**Fractional-cent aggregation edge case:** convert a batch of instructions whose `amount` values each
round to a whole number of cents independently, but whose *sum before rounding* would differ from the
*sum of the independently-rounded cents* (e.g. three instructions of `10.005`, `10.005`, `10.005` —
each rounds away-from-zero to `1001` cents individually, total `3003` cents). The self-balancing
record's amount and the trailer's credit/debit totals are computed by summing each instruction's
**already-rounded** per-instruction cents (`DirectEntryAmountTotals.SumAmountInCents`) — the same
computation used by both mappers — so they always agree with each other and with the sum of the
individual detail records' Amount fields, with no drift.

---

## F-006 — Detail record field-position verification — TC-013, TC-014, TC-015

Using the F-003 happy-path response, extract `convertedText`, split on `\r\n`, and inspect line 2
(the first real Detail Record — 1-based line 1 is the Header). Each Detail Record is exactly **120
characters**. Field positions (1-based, inclusive), using the first instruction from the F-003
two-instruction sample (`sourceBankBSB: "015141"`, `sourceBankAccountNo: "111375004"`,
`amount: 7500.0`):

| Field | Position | Width | Source | Expected value for instruction 1 |
|---|---|---|---|---|
| Record Type | 1 | 1 | constant | `1` |
| BSB Number | 2–8 | 7 | instruction `sourceBankBSB`, reformatted `nnnnnn` → `nnn-nnn` | `015-141` |
| Account Number | 9–17 | 9 | instruction `sourceBankAccountNo`, right-justified, space-padded | `111375004` (exactly 9 chars, no padding needed) |
| Indicator | 18 | 1 | constant | `N` |
| Transaction Code | 19–20 | 2 | `settings.TransactionCode` — **same value for every instruction in the batch**, not per-instruction | `13` |
| Amount (cents, zero-filled) | 21–30 | 10 | instruction `amount` × 100, rounded away-from-zero | `0000750000` |
| Title of Account | 31–62 | 32 | `settings.Title` | `SHAW AND PARTNERS LIMITED` + trailing spaces |
| Lodgement Reference | 63–80 | 18 | `settings.LodgementReferenceDetails` | `PAYMENTS` + trailing spaces |
| Trace BSB | 81–87 | 7 | `settings.TraceAccountBsb` | `062-000` |
| Trace Account Number | 88–96 | 9 | `settings.TraceAccountAccNo`, right-justified, space-padded | `21120227` (space-padded left by 1) |
| Remitter Name | 97–112 | 16 | `settings.NameOfRemitter` | `SHAW AND PARTNER` + trailing spaces |
| Withholding Tax Amount | 113–120 | 8 | `settings.AmountOfWithholdingTax` | `00000000` |

Verify with PowerShell (after saving `convertedText` to `$text`):
```powershell
$lines = $text -split "`r`n"
$detail1 = $lines[1]
$detail1.Length            # expect 120
$detail1.Substring(0,1)    # "1"
$detail1.Substring(1,7)    # "015-141"
$detail1.Substring(18,2)   # "13"
$detail1.Substring(20,10)  # "0000750000"
```

**Amount boundary case:** convert an instruction with `amount = 1.00` and confirm the amount field is
exactly `0000000100` (right-justified, zero-filled, still 10 chars).

**Fractional-cent rounding case:** convert an instruction with `amount = 10.005` and confirm the
amount field is `0000001001` (rounds away-from-zero to `10.01` → `1001` cents).

**Account number boundary case:** convert two instructions — one with `sourceBankAccountNo` at
exactly 9 characters (`"123456789"`, needs no padding) and one at 4 characters (`"1000"`, expect
`     1000`, right-justified with 5 leading spaces).

---

## F-007 — Header/Trailer assembly + totals reconciliation — TC-016, TC-017, TC-018

**Header Record** (line 1 of `convertedText`), 120 characters — every field is either a constant or
sourced from `DirectEntry` configuration, except the processed date:

| Field | Position | Width | Source | Expected value (checked-in `appsettings.json`) |
|---|---|---|---|---|
| Record Type | 1 | 1 | constant | `0` |
| Blank | 2–18 | 17 | constant | spaces |
| Reel Sequence Number | 19–20 | 2 | constant | `01` |
| Name of User Financial Institution | 21–23 | 3 | `settings.InstitutionCode` | `CBA` |
| Blank | 24–30 | 7 | constant | spaces |
| Name of User Supplying File | 31–56 | 26 | `settings.NameOfUserSupplyingFile` | `SHAW AND PARTNERS LIMITED` |
| User Identification Number | 57–62 | 6 | `settings.UserIdentificationNumber`, zero-padded left | `301500` |
| Description of Entries | 63–74 | 12 | `settings.DescriptionOfEntriesOnFile` (truncated if longer than 12) | `ONLINEPAYMEN` (config value `ONLINEPAYMENTS` truncated to 12 chars) |
| Date to be Processed (DDMMYY) | 75–80 | 6 | **earliest** instruction's `paymentDate` in the batch | e.g. `200826` for `paymentDate: "2026-08-20T10:00:00"` |
| Blank | 81–120 | 40 | constant | spaces |

**Trailer Record** (last line of `convertedText`), 120 characters, computed from
`DirectEntryAmountTotals.SumAmountInCents` over the real instructions — F-014 makes the trailer
always self-balance:

| Field | Position | Width | Expected value (F-003 sample: 7500.00 + 12500.00 = 20000.00) |
|---|---|---|---|
| Record Type | 1 | 1 | `7` |
| BSB Number (placeholder) | 2–8 | 7 | `999-999` |
| Blank | 9–20 | 12 | spaces |
| File Net Total | 21–30 | 10 | `0000000000` (always — see TC-032) |
| File Credit Total | 31–40 | 10 | `0002000000` |
| File Debit Total | 41–50 | 10 | `0002000000` (equals Credit Total — always, see TC-032) |
| Blank | 51–74 | 24 | spaces |
| Record Count (Detail Records, real + self-balancing) | 75–80 | 6 | `000003` (2 real instructions + 1 self-balancing record) |
| Blank | 81–120 | 40 | spaces |

**Empty-batch edge case:** the trailer mapper produces `0000000000` for all three totals and a record
count of `000001` (the self-balancing record alone) for an empty instruction list — but this state is
unreachable via `POST /convert` because F-005's minimum-1 rule rejects the batch first (TC-029).

Unlike the pre-F-014 contract, there is no longer a "credit-only" vs. "mixed credit/debit" batch
scenario to test: every real detail record shares the **same** `settings.TransactionCode` (it is
organisation-level config, not per-instruction), so a batch's real detail records are always all
credit or all debit together — only the self-balancing record posts the opposite direction.

---

## F-008 — Full assembly structural checks (happy path E2E) — TC-019, TC-020

Using the F-003 response's `convertedText`:

1. Split on `\r\n` (do not use `\n` alone — every record must be CRLF-terminated per the spec).
2. Confirm the number of non-empty lines = `instructions.Count + 3` (1 header + N real details + 1
   self-balancing detail + 1 trailer).
3. Confirm every line is exactly 120 characters.
4. Confirm line 1 starts with `0` (header), the last content line starts with `7` (trailer), and every
   line in between starts with `1` (detail, including the self-balancing record).
5. Cross-check the trailer's Record Count field (`instructions.Count + 1`) against the number of `1`
   lines.
6. Confirm the response JSON has `success: true` and `errors: null` (TC-020, ADR-008).

```powershell
$lines = $text -split "`r`n" | Where-Object { $_ -ne "" }
$lines.Count                                   # expect instructions.Count + 3
$lines | ForEach-Object { $_.Length }           # expect 120 for every line
$lines[0].Substring(0,1)                        # "0"
$lines[-1].Substring(0,1)                       # "7"
```

---

## Boundary-length field reference

Use these to construct additional edge-case requests as needed; each is the maximum allowed length
before the corresponding F-005 rule rejects the batch (see the F-005 table above for the "over the
limit" mutations). Only fields still present on `PaymentInstructionRequest` are validated per
instruction — everything else is fixed, organisation-level configuration (see the "Contract note" at
the top of this document).

| Field | Max length / bound |
|---|---|
| `sourceBankBSB` | exactly 6 numeric digits (no hyphen) |
| `sourceBankAccountNo` | 9 characters; digits/letters/hyphens/spaces only, not all-zero, not all-blank |
| `amount` | greater than 0, up to 99,999,999.99 (converts to at most 10 digits of cents) |
| `accountNo` | must not be blank (no length limit enforced) |
| minimum batch size | 1 payment instruction (reduced from 2 by F-014) |
