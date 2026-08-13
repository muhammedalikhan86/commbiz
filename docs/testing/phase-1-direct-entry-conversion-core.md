# Test Runbook: Phase 1 — Direct Entry Conversion Core

> Status: DRAFT
> Version: v1
> Last updated: 2026-08-13
> Covers: F-001–F-008 (see [docs/project-management.md](../project-management.md))
> Source data: [docs/stash/Direct Entry - File Specification CommBiz.md](../stash/Direct%20Entry%20-%20File%20Specification%20CommBiz.md) §5 (Sample File)

This runbook is a step-by-step manual verification guide for the full Direct Entry conversion
pipeline: **Host (Kestrel) → Wolverine dispatch → Payment Type Router (F-004) → Validator (F-005) →
Detail/Header/Trailer mapping (F-006/F-007) → assembled response (F-008)**. Each section can be run
independently against a locally running instance of the API.

## ⚠️ Known caveat: rejection responses return HTTP 200, not 4xx

Every example below that describes a "rejected batch" still returns **`200 OK`** at the transport
level — the request reached the endpoint and was handled without a routing/server error. Rejection is
signalled only in the JSON body via `"success": false` plus a populated `errors` array. This is a known
open product-decision item (not yet reconciled with `docs/test-cases.md`, which currently describes
rejection paths as "4xx"), not a bug in this runbook or in the implementation it describes. Treat every
`200 OK` + `"success": false` response below as the expected, correct result for that scenario.

## Prerequisites

1. Start the host from the repo root:
   ```powershell
   dotnet run --project src/CommBiz.Api
   ```
2. Confirm Kestrel is listening on `http://localhost:5182` (per
   [src/CommBiz.Api/Properties/launchSettings.json](../../src/CommBiz.Api/Properties/launchSettings.json), `http` profile).
3. All examples below use PowerShell's `Invoke-RestMethod`. A `curl.exe` equivalent is noted where useful.

---

## F-001 — Host scaffold smoke test

```powershell
Invoke-RestMethod -Uri http://localhost:5182/health -Method Get
```

**Expected:** `200 OK`, body `{"status":"Healthy"}`.

---

## F-002 — Wolverine wiring smoke test

```powershell
Invoke-RestMethod -Uri http://localhost:5182/diagnostics/ping -Method Post `
  -ContentType 'application/json' `
  -Body '{"message":"hello"}'
```

**Expected:** `200 OK`, body echoes the message plus a UTC timestamp, e.g.:
```json
{ "message": "hello", "respondedAtUtc": "2026-08-13T04:12:00.000Z" }
```
This confirms the request was dispatched through Wolverine's `IMessageBus.InvokeAsync`, not just
routed by Minimal API directly.

---

## F-003 — Happy path: single valid batch (baseline)

Sample data below is drawn directly from the Direct Entry spec's §5 Sample File (two detail records,
satisfying the minimum-2-instruction rule from F-008/F-005).

```powershell
$body = @'
{
  "fileName": "COMPANY ABCD PTY LTD",
  "userIdentificationNumber": "301500",
  "descriptionOfEntries": "EFT-PAYMENT",
  "dateToBeProcessed": "2006-12-05",
  "instructions": [
    {
      "paymentType": "DirectEntry",
      "bsb": "062-000",
      "accountNumber": "10001000",
      "indicator": "",
      "transactionCode": "53",
      "amountInCents": 10050,
      "accountTitle": "CLIENT COMPANY XYZ",
      "lodgementReference": "INVOICE 123456",
      "traceBsb": "063-000",
      "traceAccountNumber": "100000000",
      "remitterName": "COMPANY ABCD P/L",
      "withholdingTaxAmountInCents": 0
    },
    {
      "paymentType": "DirectEntry",
      "bsb": "063-000",
      "accountNumber": "10000000",
      "indicator": "",
      "transactionCode": "13",
      "amountInCents": 10050,
      "accountTitle": "COMPANY ABCD PTY LTD",
      "lodgementReference": "PAYMENT",
      "traceBsb": "063-000",
      "traceAccountNumber": "100000000",
      "remitterName": "COMPANY ABCD P/L",
      "withholdingTaxAmountInCents": 0
    }
  ]
}
'@

Invoke-RestMethod -Uri http://localhost:5182/direct-entry/convert -Method Post `
  -ContentType 'application/json' -Body $body
```

**Expected:** `200 OK`, JSON envelope shaped as:
```json
{
  "success": true,
  "convertedText": "<header record>\r\n<detail 1>\r\n<detail 2>\r\n<trailer record>\r\n",
  "errors": null
}
```
Per ADR-008, `convertedText` is the full converted file inline — no download link. See F-006/F-007
below for how to verify the exact field-level content of `convertedText`.

This request/response pair is the baseline for every other scenario in this runbook — each edge case
below is a small mutation of this same body.

---

## F-004 — Payment Type Router: unsupported payment type rejects the whole batch

Take the F-003 body and change the **first** instruction's `paymentType` to `"BPAY"` (anything other
than `"DirectEntry"`, case-insensitively):

```powershell
# ...same body as F-003, but instructions[0].paymentType = "BPAY"
Invoke-RestMethod -Uri http://localhost:5182/direct-entry/convert -Method Post `
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
converted. `index` is the 0-based position of the offending instruction in the `instructions` array.

Router check runs **before** field validation (F-005) — an unsupported-type instruction with an
otherwise-invalid BSB will still only report the payment-type error, not both.

---

## F-005 — Field validation: one scenario per rule

For each row, start from the F-003 happy-path body and apply only the described mutation to
**one field on one instruction** (or the header, for header-level rules), then POST. All of these
return `200 OK` with `"success": false`, `"convertedText": null`, and an `errors` array containing an
entry whose `reason` matches the pattern shown. Header-level failures use `"index": -1`.

| Rule | Mutation | Expected `errors[].reason` (substring) | `index` |
|---|---|---|---|
| BSB format | `instructions[0].bsb = "62000"` (missing hyphen) | `must match format nnn-nnn` | `0` |
| Trace BSB format | `instructions[0].traceBsb = "63000"` | `must match format nnn-nnn` | `0` |
| Account number all-zero | `instructions[0].accountNumber = "00000000"` | `is invalid` | `0` |
| Account number all-blank | `instructions[0].accountNumber = "   "` | `is invalid` | `0` |
| Account number too long | `instructions[0].accountNumber = "1234567890"` (10 chars) | `is invalid` | `0` |
| Trace account number invalid | `instructions[0].traceAccountNumber = "000000000"` | `is invalid` | `0` |
| Indicator invalid | `instructions[0].indicator = "Z"` | `must be one of N, W, X, Y or blank` | `0` |
| Transaction code invalid | `instructions[0].transactionCode = "99"` | `is not a supported code` | `0` |
| Amount negative | `instructions[0].amountInCents = -100` | `must be non-negative and at most 10 digits` | `0` |
| Amount over 10 digits | `instructions[0].amountInCents = 10000000000` | `must be non-negative and at most 10 digits` | `0` |
| AccountTitle blank | `instructions[0].accountTitle = ""` | `must not be blank` | `0` |
| AccountTitle too long | `instructions[0].accountTitle` = 33+ chars | `at most 32 characters` | `0` |
| LodgementReference too long | `instructions[0].lodgementReference` = 19+ chars | `at most 18 characters` | `0` |
| RemitterName blank | `instructions[0].remitterName = ""` | `must not be blank` | `0` |
| RemitterName too long | `instructions[0].remitterName` = 17+ chars | `at most 16 characters` | `0` |
| WithholdingTax negative | `instructions[0].withholdingTaxAmountInCents = -1` | `must be non-negative and at most 8 digits` | `0` |
| WithholdingTax over 8 digits | `instructions[0].withholdingTaxAmountInCents = 100000000` | `must be non-negative and at most 8 digits` | `0` |
| FileName blank | top-level `fileName = ""` | `must not be blank` | `-1` |
| FileName too long | top-level `fileName` = 27+ chars | `at most 26 characters` | `-1` |
| UserIdentificationNumber non-numeric | `userIdentificationNumber = "ABCDEF"` | `must be numeric` | `-1` |
| UserIdentificationNumber too long | `userIdentificationNumber = "1234567"` (7 digits) | `at most 6 digits` | `-1` |
| DescriptionOfEntries blank | `descriptionOfEntries = ""` | `must not be blank` | `-1` |
| DescriptionOfEntries too long | `descriptionOfEntries` = 13+ chars | `at most 12 characters` | `-1` |

**One-invalid-among-many (regression-sensitive):** apply only the BSB-format mutation to
`instructions[0]` in a batch that otherwise has 5+ valid instructions. Expected: the whole batch is
still rejected with exactly one error entry (`index: 0`) — the 4 remaining valid instructions must not
appear anywhere in the response.

---

## F-008 — Minimum-2-instruction rejection

Take the F-003 body and remove the second instruction, leaving only one:

**Expected:** `200 OK`,
```json
{
  "success": false,
  "convertedText": null,
  "errors": [
    { "index": -1, "reason": "Payment file must contain at least 2 detail records (found 1)." }
  ]
}
```

---

## F-006 — Detail record field-position verification

Using the F-003 happy-path response, extract `convertedText`, split on `\r\n`, and inspect line 2
(the first Detail Record — 1-based line 1 is the Header). Each Detail Record is exactly **120
characters**. Field positions (1-based, inclusive):

| Field | Position | Width | Expected value for instruction 1 |
|---|---|---|---|
| Record Type | 1 | 1 | `1` |
| BSB Number | 2–8 | 7 | `062-000` |
| Account Number | 9–17 | 9 | `10001000` (space-padded left, right-justified) |
| Indicator | 18 | 1 | ` ` (blank) |
| Transaction Code | 19–20 | 2 | `53` |
| Amount (cents, zero-filled) | 21–30 | 10 | `0000010050` |
| Title of Account | 31–62 | 32 | `CLIENT COMPANY XYZ` + trailing spaces |
| Lodgement Reference | 63–80 | 18 | `INVOICE 123456` + trailing spaces |
| Trace BSB | 81–87 | 7 | `063-000` |
| Trace Account Number | 88–96 | 9 | `100000000` |
| Remitter Name | 97–112 | 16 | `COMPANY ABCD P/L` (exactly 16 chars, no padding) |
| Withholding Tax Amount | 113–120 | 8 | `00000000` |

Verify with PowerShell (after saving `convertedText` to `$text`):
```powershell
$lines = $text -split "`r`n"
$detail1 = $lines[1]
$detail1.Length            # expect 120
$detail1.Substring(0,1)    # "1"
$detail1.Substring(1,7)    # "062-000"
$detail1.Substring(18,2)   # "53"
$detail1.Substring(20,10)  # "0000010050"
```

**Amount boundary case:** convert an instruction with `amountInCents = 100` (i.e. $1.00) and confirm
the amount field is exactly `0000000100` (right-justified, zero-filled, still 10 chars).

**Title boundary case:** convert two instructions — one with `accountTitle` at exactly 32 characters
and one at 31 characters. Confirm both produce a title field of exactly 32 characters, the 31-char one
space-padded on the right by exactly 1 space, per the position table above.

---

## F-007 — Header/Trailer assembly + totals reconciliation

**Header Record** (line 1 of `convertedText`), 120 characters:

| Field | Position | Width | Expected value (F-003 sample) |
|---|---|---|---|
| Record Type | 1 | 1 | `0` |
| Blank | 2–18 | 17 | spaces |
| Reel Sequence Number | 19–20 | 2 | `01` |
| Name of User Financial Institution | 21–23 | 3 | `CBA` |
| Blank | 24–30 | 7 | spaces |
| Name of User Supplying File (`fileName`) | 31–56 | 26 | `COMPANY ABCD PTY LTD` + trailing spaces |
| User Identification Number | 57–62 | 6 | `301500` |
| Description of Entries | 63–74 | 12 | `EFT-PAYMENT` + 1 trailing space |
| Date to be Processed (DDMMYY) | 75–80 | 6 | `051206` |
| Blank | 81–120 | 40 | spaces |

**Trailer Record** (last line of `convertedText`), 120 characters, computed from the batch's Detail
Records (self-balancing):

| Field | Position | Width | Expected value (F-003 sample: 1 credit @ 10050c, 1 debit @ 10050c) |
|---|---|---|---|
| Record Type | 1 | 1 | `7` |
| BSB Number (placeholder) | 2–8 | 7 | `999-999` |
| Blank | 9–20 | 12 | spaces |
| File Net Total (`\|credit − debit\|`) | 21–30 | 10 | `0000000000` |
| File Credit Total | 31–40 | 10 | `0000010050` |
| File Debit Total | 41–50 | 10 | `0000010050` |
| Blank | 51–74 | 24 | spaces |
| Record Count (Detail Records) | 75–80 | 6 | `000002` |
| Blank | 81–120 | 40 | spaces |

**Credit-only case:** convert a batch where every instruction's `transactionCode` is anything except
`"13"` (e.g. two instructions both coded `"53"`). Expected: Debit Total = `0000000000`; Net Total =
Credit Total.

**Mixed credit/debit case (regression-sensitive):** convert a batch with one `"13"` (debit) and two
`"53"` (credit) instructions with different amounts, e.g. debit 5000c, credits 3000c + 4000c. Expected:
Credit Total = `0000007000`, Debit Total = `0000005000`, Net Total = `0000002000` (unsigned
`|7000 − 5000|`), Record Count = `000003`.

---

## F-008 — Full assembly structural checks (happy path E2E)

Using the F-003 response's `convertedText`:

1. Split on `\r\n` (do not use `\n` alone — every record must be CRLF-terminated per the spec).
2. Confirm the number of non-empty lines = `instructions.Count + 2` (1 header + N details + 1 trailer).
3. Confirm every line is exactly 120 characters.
4. Confirm line 1 starts with `0` (header), the last content line starts with `7` (trailer), and all
   lines in between start with `1` (detail).
5. Cross-check the trailer's Record Count field against the actual number of detail lines.

```powershell
$lines = $text -split "`r`n" | Where-Object { $_ -ne "" }
$lines.Count                                   # expect instructions.Count + 2
$lines | ForEach-Object { $_.Length }           # expect 120 for every line
$lines[0].Substring(0,1)                        # "0"
$lines[-1].Substring(0,1)                       # "7"
```

---

## Boundary-length field reference

Use these to construct additional edge-case requests as needed; each is the maximum allowed length
before the corresponding F-005 rule rejects the batch (see the F-005 table above for the "over the
limit" mutations):

| Field | Max length / bound |
|---|---|
| `fileName` | 26 characters |
| `userIdentificationNumber` | 6 digits |
| `descriptionOfEntries` | 12 characters |
| `accountNumber` / `traceAccountNumber` | 9 characters |
| `accountTitle` | 32 characters |
| `lodgementReference` | 18 characters |
| `remitterName` | 16 characters |
| `amountInCents` | 0 to 9,999,999,999 (10 digits) |
| `withholdingTaxAmountInCents` | 0 to 99,999,999 (8 digits) |
