# Test Runbook: Phase 2 — Additional Payment Types I

> Status: DRAFT
> Version: v4
> Last updated: 2026-08-18
> Covers: F-015–F-018, F-021, F-022, F-023 (see [docs/project-management.md](../project-management.md))
> Scenario reference: [docs/test-cases.md](../test-cases.md) TC-051–TC-065 (independently numbered
> range, disjoint from test-cases.md's own TC-033–TC-050 Phase 2 section — some scenarios below are
> manual walkthroughs of the same underlying rules test-cases.md covers, e.g. TC-051/TC-052 here mirror
> test-cases.md's TC-004/TC-005 mixed-type/unsupported-type rejections); this runbook's own TC-066–TC-070
> (F-021 `Mappings` field) and TC-075–TC-084 (F-022/F-023 FOREX routing + FX conversion) have no
> `test-cases.md` counterpart yet — tracked as PM-009
> Source data: [docs/stash/BPay Payments - CommBiz File Specification.md](../stash/BPay%20Payments%20-%20CommBiz%20File%20Specification.md) §1.1/§1.3,
> [docs/stash/CommBiz File Specification - International Money Transfers Priority Payments Non CBA Payment Requests (MT101) v9.md](../stash/CommBiz%20File%20Specification%20-%20International%20Money%20Transfers%20Priority%20Payments%20Non%20CBA%20Payment%20Requests%20%28MT101%29%20v9.md) §1.2/§1.4,
> [docs/stash/CommBiz IPFX Bulk Settlement Upload - File Specification v4.0 2.md](../stash/CommBiz%20IPFX%20Bulk%20Settlement%20Upload%20-%20File%20Specification%20v4.0%202.md) "File Description and Business Rules" / "File Contents - Data Rows Format"

This runbook is a step-by-step manual verification guide for the Payment Type Router's promotion to a
real cross-slice dispatcher and the two payment types it dispatches to besides Direct Entry: **Host
(Kestrel) → Wolverine dispatch → Payment Type Router (F-015, `Features/PaymentRouting`) → BPAY slice
(F-016, `Features/BPay`) / IMT slice (F-017, `Features/Imt`) → assembled response**. Each section can be
run independently against a locally running instance of the API. For the Direct Entry-specific
pipeline, see [phase-1-direct-entry-conversion-core.md](phase-1-direct-entry-conversion-core.md).

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
3. All examples below use PowerShell's `Invoke-RestMethod`. Ready-to-run equivalents for every scenario
   below also live in [tests/smoke/BPay.http](../../tests/smoke/BPay.http),
   [tests/smoke/Imt.http](../../tests/smoke/Imt.http),
   [tests/smoke/PriorityPayments.http](../../tests/smoke/PriorityPayments.http), and
   [tests/smoke/Errors.http](../../tests/smoke/Errors.http). **No `tests/smoke/Fx.http` file exists
   yet** — the F-022/F-023 FX scenarios below (TC-075–TC-084) are written directly in this runbook
   instead; creating an `Fx.http` mirroring the others is tracked as a follow-up, not fabricated here.
4. `appsettings.json`'s `BPay` section (`FundingAccount`, `FileNumber`) uses real, confirmed values
   (per [Features/BPay/README.md](../../src/CommBiz.Api/Features/BPay/README.md)) — same settlement
   account IMT/Priority Payments/FX already use. `appsettings.json`'s `Imt` section
   (`DebitAccountBsb`, `DebitAccountNumber`, `DebitAccountName`) also uses
   real, confirmed values (per [Features/Imt/README.md](../../src/CommBiz.Api/Features/Imt/README.md)).

---

## F-015 — Payment Type Router: batch-level rejections and case-insensitive dispatch — TC-051–TC-055

`PaymentTypeRouter` peeks `paymentTypeCode` on the raw JSON batch (matching the property name
case-insensitively), rejects at the batch level before any slice's own validator ever runs, then
uppercases every instruction's type code before comparing against `DE`/`BPAY`/`TT` — so the *value* is
also matched case-insensitively, not just the property name.

**Automated equivalent:** [tests/CommBiz.Api.Tests/PaymentRouting/PaymentTypeRouterTests.cs](../../tests/CommBiz.Api.Tests/PaymentRouting/PaymentTypeRouterTests.cs)

### TC-051 — Mixed payment types rejects the whole batch

From [tests/smoke/Errors.http](../../tests/smoke/Errors.http) (verbatim):

```powershell
$body = @'
[
  {
    "paymentTypeCode": "BPAY",
    "accountNo": "S1605677",
    "paymentDate": "2026-08-20T10:00:00",
    "amount": 7500.0,
  },
  {
    "paymentTypeCode": "DE",
    "accountNo": "S1605678",
    "paymentDate": "2026-08-20T10:00:00",
    "amount": 12500.0,
  }
]
'@

Invoke-RestMethod -Uri http://localhost:5182/convert -Method Post `
  -ContentType 'application/json' -Body $body
```

**Expected:** `200 OK`,
```json
{
  "success": false,
  "convertedText": null,
  "errors": [
    { "index": -1, "reason": "Payment batch must not mix payment types (found 'BPAY', 'DE')." }
  ]
}
```
Neither instruction is partially converted — the router rejects the batch before dispatching to either
slice's validator.

### TC-052 — Single unsupported payment type rejects whole batch

From [tests/smoke/Errors.http](../../tests/smoke/Errors.http) (verbatim):

```powershell
$body = @'
[
  {
    "paymentTypeCode": "XX",
    "accountNo": "S1605677",
    "paymentDate": "2026-08-20T10:00:00",
    "amount": 7500.0,
  }
]
'@

Invoke-RestMethod -Uri http://localhost:5182/convert -Method Post `
  -ContentType 'application/json' -Body $body
```

**Expected:** `200 OK`,
```json
{
  "success": false,
  "convertedText": null,
  "errors": [
    { "index": 0, "reason": "Unsupported payment type 'XX'." }
  ]
}
```

### TC-053 — Case-insensitive dispatch: BPAY

Take the single-instruction happy-path body from [tests/smoke/BPay.http](../../tests/smoke/BPay.http)
and lower-case only `PaymentTypeCode`:

```powershell
$body = @'
[
  {
    "PaymentTypeCode": "bpay",
    "AccountNo": "S1218944",
    "PaymentDate": "2026-08-11T11:45:00",
    "Amount": 15000.00,
    "BPayBillerCode": "488577",
    "BPayReference": "1202194308172125"
  }
]
'@

Invoke-RestMethod -Uri http://localhost:5182/convert -Method Post `
  -ContentType 'application/json' -Body $body
```

**Expected:** `200 OK`, `success: true`, identical to the `"BPAY"` (upper-case) equivalent — router
dispatch is unaffected by the value's casing. `convertedText` splits into 2 lines (header + 1 detail).

### TC-054 — Case-insensitive dispatch: IMT (routing code `TT`)

Take the single-instruction happy-path body from [tests/smoke/Imt.http](../../tests/smoke/Imt.http) and
lower-case only `paymentTypeCode`:

```powershell
$body = @'
[
  {
    "paymentTypeCode": "tt",
    "destinationBankAccountName": "SAMER MOHAMMED KIKI",
    "destinationBankAccountNo": "658450191",
    "paymentDate": "2026-08-18T10:00:00",
    "sourceCurrency": "USD",
    "sourceAmount": 588517.58,
    "amount": 0.0,
    "paymentReference": "TT-000001",
    "notes": "10 bps on fx",
    "destinationBankSWIFTCode": "CHASUS33",
    "destinationBankName": "NATIONAL FINANCIAL SERVICES",
    "beneficiaryAddress": "9101 Alta Drive, U15, Las Vegas, NV 89145",
    "intermediaryBankSWIFTCode": "CHASUS33",
    "intermediaryBankName": "CHASE MANHATTAN BANK",
  }
]
'@

Invoke-RestMethod -Uri http://localhost:5182/convert -Method Post `
  -ContentType 'application/json' -Body $body
```

**Expected:** `200 OK`, `success: true`, identical to the `"TT"` (upper-case) equivalent —
`convertedText` is a single 27-field CSV row (field 1, the file's own Transaction Type, is still the
constant `"IMT"` regardless of the routing code's casing).

### TC-055 — Case-insensitive dispatch: Direct Entry

Take the single-instruction happy-path body used in
[phase-1-direct-entry-conversion-core.md](phase-1-direct-entry-conversion-core.md) F-002/F-003
(sourced from [tests/smoke/DirectEntry.http](../../tests/smoke/DirectEntry.http)) and lower-case only
`paymentTypeCode`:

```powershell
$body = @'
[
  {
    "paymentTypeCode": "de",
    "accountNo": "S1605677",
    "paymentDate": "2026-08-20T10:00:00",
    "amount": 7500.0,
  }
]
'@

Invoke-RestMethod -Uri http://localhost:5182/convert -Method Post `
  -ContentType 'application/json' -Body $body
```

**Expected:** `200 OK`, `success: true`, identical to the `"DE"` (upper-case) equivalent —
`convertedText` splits into 4 lines (header + 1 real detail + 1 self-balancing detail + 1 trailer, per
F-014).

---

## F-016 — BPAY Batch Payments conversion — TC-056–TC-060

BPAY's output is CSV (comma-delimited, not fixed-width), and — unlike Direct Entry — has **no
trailer or self-balancing record**: the file is simply a Header record (`RecordType "01"`) followed by
one Payment Details record (`RecordType "50"`) per instruction, each CRLF-terminated.

**Automated equivalents:**
[tests/CommBiz.Api.Tests/BPay/BPayValidatorTests.cs](../../tests/CommBiz.Api.Tests/BPay/BPayValidatorTests.cs),
[tests/CommBiz.Api.Tests/BPay/BPayConvertEndpointTests.cs](../../tests/CommBiz.Api.Tests/BPay/BPayConvertEndpointTests.cs)

**Header Record** (line 1 of `convertedText`), comma-delimited, 8 fields — every field is either a
constant, `BPaySettings`-sourced, or the batch's earliest `PaymentDate`/summed cents total:

| Field | CSV position | Source | Expected value (TC-056 instruction 1) |
|---|---|---|---|
| Record Type | 1 | constant | `01` |
| File Creation Date | 2 | `DateTime.UtcNow` | e.g. `20260811` |
| File Creation Time | 3 | `DateTime.UtcNow` | e.g. `120000` |
| File Number | 4 | `settings.FileNumber` (checked-in `appsettings.json`) | `001` |
| Payment Account | 5 | `settings.FundingAccount` (checked-in `appsettings.json`) | `06200021120075` |
| Payment Date | 6 | **earliest** instruction's `PaymentDate` in the batch | `20260811` |
| Number of Payment Records | 7 | `instructions.Count` | `2` |
| Total Amount of Payments | 8 | sum of `AmountToCents(instruction.Amount)` over the batch | `282590` |

**Payment Details Record** (one line per instruction), comma-delimited, 25 fields — unlike Direct
Entry there is no trailer/self-balancing record to reconcile against, so every position the spec
doesn't assign to this record type is simply left blank:

| Field | CSV position | Source | Expected value (TC-056 instruction 1) |
|---|---|---|---|
| Record Type | 1 | constant | `50` |
| File Creation Date | 2 | reserved (blank on detail records) | empty |
| File Creation Time | 3 | reserved | empty |
| File Number | 4 | reserved | empty |
| Payment Account | 5 | reserved | empty |
| Payment Date | 6 | reserved | empty |
| Number of Payment Records | 7 | reserved | empty |
| Currency Code of Payment | 8 | reserved | empty |
| Biller Code | 9 | `instruction.BPayBillerCode` | `488577` |
| Service Code | 10 | reserved | empty |
| Customer Reference Number | 11 | `instruction.BPayReference` | `1202194308172126` |
| Payment Method | 12 | reserved | empty |
| Entry Method | 13 | reserved | empty |
| Amount | 14 | `AmountToCents(instruction.Amount)` | `62590` |
| Transaction Reference Number | 15 | reserved | empty |
| Original Reference Number | 16 | reserved | empty |
| BPAY Settlement Date | 17 | reserved | empty |
| Date Payment Accepted | 18 | reserved | empty |
| Time Payment Accepted | 19 | reserved | empty |
| Payer Name | 20 | reserved | empty |
| Additional Reference Code | 21 | reserved | empty |
| Error Correction Reason | 22 | reserved | empty |
| Discount Method | 23 | reserved | empty |
| Discount Reference | 24 | reserved | empty |
| Discretionary Data | 25 | reserved | empty |

### TC-056 — Happy path: 2 valid instructions

From [tests/smoke/BPay.http](../../tests/smoke/BPay.http) (verbatim):

```powershell
$body = @'
[
  {
    "PaymentTypeCode": "BPAY",
    "AccountNo": "S1218945",
    "PaymentDate": "2026-08-11T12:00:00",
    "Amount": 625.90,
    "BPayBillerCode": "488577",
    "BPayReference": "1202194308172126"
  },
  {
    "PaymentTypeCode": "BPAY",
    "AccountNo": "S1218946",
    "PaymentDate": "2026-08-11T12:15:00",
    "Amount": 2200.00,
    "BPayBillerCode": "488577",
    "BPayReference": "1202194308172127"
  }
]
'@

Invoke-RestMethod -Uri http://localhost:5182/convert -Method Post `
  -ContentType 'application/json' -Body $body
```

**Expected:** `200 OK`,
```json
{
  "success": true,
  "convertedText": "01,<date>,<time>,<FileNumber>,<FundingAccount>,20260811,2,282590\r\n50,,,,,,,,488577,,1202194308172126,,,62590,,,,,,,,,,,\r\n50,,,,,,,,488577,,1202194308172127,,,220000,,,,,,,,,,,\r\n",
  "errors": null
}
```
Splitting `convertedText` on `\r\n` (dropping the trailing empty element) yields exactly **3** records:
1 header + 2 details, no trailer. The header's field 7 (total amount in cents) is `282590` = `62590 +
220000`, and field 6 (instruction count) is `2`.

```powershell
$lines = $text -split "`r`n" | Where-Object { $_ -ne "" }
$lines.Count                              # expect 3 (header + 2 details, no trailer)
$lines[0].Split(',')[0]                   # "01" (header record type)
$lines[1].Split(',')[0]                   # "50" (detail record type)
$lines[1].Split(',')[8]                   # "488577" (field 9: Biller Code)
$lines[1].Split(',')[10]                  # "1202194308172126" (field 11: Customer Reference Number)
$lines[1].Split(',')[13]                  # "62590" (field 14: Amount, in cents)
```

### TC-057 — Happy path: single-instruction batch (BPAY's own minimum, unrelated to F-014)

From [tests/smoke/BPay.http](../../tests/smoke/BPay.http) (verbatim). BPay's minimum is 1
(`BPayValidator.MinimumInstructionCount`) — BPay has no self-balancing record, so unlike Direct Entry
this was never a minimum-2 rule to begin with:

```powershell
$body = @'
[
  {
    "PaymentTypeCode": "BPAY",
    "AccountNo": "S1218944",
    "PaymentDate": "2026-08-11T11:45:00",
    "Amount": 15000.00,
    "BPayBillerCode": "488577",
    "BPayReference": "1202194308172125"
  }
]
'@

Invoke-RestMethod -Uri http://localhost:5182/convert -Method Post `
  -ContentType 'application/json' -Body $body
```

**Expected:** `200 OK`, `success: true`; `convertedText` splits into exactly **2** non-empty lines
(header + 1 detail).

### TC-058/TC-059/TC-060 — Field validation failures

Start from the TC-057 single-instruction body and apply only the described mutation to one field, then
POST. All return `200 OK` with `"success": false`, `"convertedText": null`.

| TC | Mutation | Expected `errors[0].reason` | `index` |
|---|---|---|---|
| TC-058 | `AccountNo = ""` | `AccountNo must not be blank.` | `0` |
| TC-059 | `BPayBillerCode = "48857A"` (non-numeric) | `BPayBillerCode '48857A' must be numeric, 1-10 digits.` | `0` |
| TC-060 | `Amount = 0` | `Amount '0' must be positive and convert to at most 12 digits of cents.` | `0` |

```powershell
$body = @'
[
  {
    "PaymentTypeCode": "BPAY",
    "AccountNo": "",
    "PaymentDate": "2026-08-11T11:45:00",
    "Amount": 15000.00,
    "BPayBillerCode": "488577",
    "BPayReference": "1202194308172125"
  }
]
'@

Invoke-RestMethod -Uri http://localhost:5182/convert -Method Post `
  -ContentType 'application/json' -Body $body
```

**Expected (TC-058):**
```json
{
  "success": false,
  "convertedText": null,
  "errors": [
    { "index": 0, "reason": "AccountNo must not be blank." }
  ]
}
```

---

## F-017 — IMT (International Money Transfers) conversion — TC-061–TC-065

The IMT file has **no header or trailer record** — it is one 27-field CSV row per instruction,
CRLF-separated, with **no trailing CRLF after the last row**. The API's routing code `TT` (Shaw and
Partners' internal "Telegraphic Transfer" name) maps to the file's own Transaction Type field (field 1),
which always writes the literal constant `"IMT"`.

**Automated equivalents:**
[tests/CommBiz.Api.Tests/Imt/ImtValidatorTests.cs](../../tests/CommBiz.Api.Tests/Imt/ImtValidatorTests.cs),
[tests/CommBiz.Api.Tests/Imt/ImtConvertEndpointTests.cs](../../tests/CommBiz.Api.Tests/Imt/ImtConvertEndpointTests.cs)

### TC-061 — Happy path: single instruction

From [tests/smoke/Imt.http](../../tests/smoke/Imt.http) (verbatim):

```powershell
$body = @'
[
  {
    "paymentTypeCode": "TT",
    "destinationBankAccountName": "SAMER MOHAMMED KIKI",
    "destinationBankAccountNo": "658450191",
    "paymentDate": "2026-08-18T10:00:00",
    "sourceCurrency": "USD",
    "sourceAmount": 588517.58,
    "amount": 0.0,
    "paymentReference": "TT-000001",
    "notes": "10 bps on fx",
    "destinationBankSWIFTCode": "CHASUS33",
    "destinationBankName": "NATIONAL FINANCIAL SERVICES",
    "beneficiaryAddress": "9101 Alta Drive, U15, Las Vegas, NV 89145",
    "intermediaryBankSWIFTCode": "CHASUS33",
    "intermediaryBankName": "CHASE MANHATTAN BANK",
  }
]
'@

Invoke-RestMethod -Uri http://localhost:5182/convert -Method Post `
  -ContentType 'application/json' -Body $body
```

**Expected:** `200 OK`, `success: true`; `convertedText` is a single row, no trailing `\r\n`:
```powershell
$text.Contains("`r`n")     # False — single row, nothing to separate
$fields = $text.Split(',')
$fields.Count              # expect 27
$fields[0]                 # "IMT" (field 1: Transaction Type — never "TT")
$fields[12]                # "US" (field 13: Intermediary Institution Country, derived from CHASUS33)
$fields[16]                # "US" (field 17: Beneficiary Bank Country, derived from CHASUS33)
```

### TC-062 — Happy path: 2 instructions, one with no intermediary bank, address sanitized

From [tests/smoke/Imt.http](../../tests/smoke/Imt.http) (verbatim) — second instruction has fields
10–13 (intermediary bank) entirely blank, a second currency (GBP), and a beneficiary address containing
a comma (`"27 Oxford Street, London W1D 2DZ"`) that must be sanitized (comma → space), not rejected:

```powershell
$body = @'
[
  {
    "paymentTypeCode": "TT",
    "destinationBankAccountName": "SAMER MOHAMMED KIKI",
    "destinationBankAccountNo": "658450191",
    "paymentDate": "2026-08-18T10:00:00",
    "sourceCurrency": "USD",
    "sourceAmount": 588517.58,
    "amount": 0.0,
    "paymentReference": "TT-000001",
    "notes": "10 bps on fx",
    "destinationBankSWIFTCode": "CHASUS33",
    "destinationBankName": "NATIONAL FINANCIAL SERVICES",
    "beneficiaryAddress": "9101 Alta Drive, U 15, Las Vegas, NV 89145",
    "intermediaryBankSWIFTCode": "CHASUS33",
    "intermediaryBankName": "CHASE MANHATTAN BANK",
  },
  {
    "paymentTypeCode": "TT",
    "destinationBankAccountName": "GLOBEX TRADING PTY LTD",
    "destinationBankAccountNo": "4471820033",
    "paymentDate": "2026-08-18T10:00:00",
    "sourceCurrency": "GBP",
    "sourceAmount": 42500.00,
    "amount": 0.0,
    "paymentReference": "TT-000002",
    "notes": "Supplier invoice settlement",
    "destinationBankSWIFTCode": "BARCGB22",
    "destinationBankName": "BARCLAYS BANK UK PLC",
    "beneficiaryAddress": "27 Oxford Street, London W1D 2DZ",
    "intermediaryBankSWIFTCode": null,
    "intermediaryBankName": null,
  }
]
'@

Invoke-RestMethod -Uri http://localhost:5182/convert -Method Post `
  -ContentType 'application/json' -Body $body
```

**Expected:** `200 OK`, `success: true`; `convertedText` is two rows joined by exactly one `\r\n`, no
trailing `\r\n`:
```powershell
$rows = $text -split "`r`n"
$rows.Count                       # expect 2
$rows[1].Split(',')[9]            # "" (field 10: Intermediary Bank Bank Code — blank)
$rows[1].Split(',')[19]           # "27 Oxford Street London W1D 2DZ" (field 20, comma sanitized to space)
```

### TC-063/TC-064/TC-065 — Field validation failures

From [tests/smoke/Errors.http](../../tests/smoke/Errors.http) (verbatim, each is TC-061's body with
exactly one field mutated).

**TC-063 — blank Notes (Transaction Description, field 2):**

```powershell
$body = @'
[
  {
    "paymentTypeCode": "TT",
    "destinationBankAccountName": "SAMER MOHAMMED KIKI",
    "destinationBankAccountNo": "658450191",
    "paymentDate": "2026-08-18T10:00:00",
    "sourceCurrency": "USD",
    "sourceAmount": 588517.58,
    "amount": 0.0,
    "paymentReference": "TT-000001",
    "notes": "",
    "destinationBankSWIFTCode": "CHASUS33",
    "destinationBankName": "NATIONAL FINANCIAL SERVICES",
    "beneficiaryAddress": "9101 Alta Drive, U15, Las Vegas, NV 89145",
    "intermediaryBankSWIFTCode": "CHASUS33",
    "intermediaryBankName": "CHASE MANHATTAN BANK (J.P. MORGAN CHASE & CO)",
  }
]
'@

Invoke-RestMethod -Uri http://localhost:5182/convert -Method Post `
  -ContentType 'application/json' -Body $body
```

**Expected:**
```json
{
  "success": false,
  "convertedText": null,
  "errors": [
    { "index": 0, "reason": "Notes (Transaction Description) must not be blank." }
  ]
}
```

**TC-064 — both SourceAmount (Payment Amount) and Amount (Debit Amount) populated:** same body as
TC-063 but `notes: "10 bps on fx"` (restored) and `amount: 250.00` (instead of `0.0`).

**Expected:**
```json
{
  "success": false,
  "convertedText": null,
  "errors": [
    { "index": 0, "reason": "Exactly one of SourceAmount (Payment Amount) or Amount (Debit Amount) must be greater than zero, not both." }
  ]
}
```

**TC-065 — disallowed character (ampersand) in Beneficiary Account Name:** same body as TC-063 but
`notes: "10 bps on fx"` (restored) and `destinationBankAccountName: "SAMER & KIKI TRADING"`.

**Expected:**
```json
{
  "success": false,
  "convertedText": null,
  "errors": [
    { "index": 0, "reason": "DestinationBankAccountName 'SAMER & KIKI TRADING' must contain at least one letter, use only letters/digits/spaces/hyphens/apostrophes, and be at most 62 characters." }
  ]
}
```
Note this is a **reject**, not a sanitize — legal/identity data (account names, account numbers, bank
names, SWIFT codes) is rejected outright on disallowed characters, unlike free-text postal/reference
fields (Beneficiary Address, Beneficiary Payment Details — see TC-062), which are sanitized instead.

---

## Boundary/limit field reference

| Field | Rule |
|---|---|
| BPAY `BPayBillerCode` | numeric only, 1–10 digits |
| BPAY `BPayReference` | numeric only, 1–20 digits |
| BPAY `Amount` | positive, up to 12 digits of cents (`9,999,999,999.99`) |
| BPAY batch size | 1–200 instructions |
| IMT `Notes` (Transaction Description) | must not be blank |
| IMT `PaymentDate` | today .. today + 7 days |
| IMT `SourceCurrency` | exactly 3 upper-case letters |
| IMT amount fields (`SourceAmount`/`Amount`) | exactly one must be > 0; up to 11 integer digits, 2 decimal digits |
| IMT SWIFT codes (`DestinationBankSWIFTCode`, `IntermediaryBankSWIFTCode`) | 8 or 11 alphanumeric characters |
| IMT `DestinationBankAccountNo` | 1–34 characters, no spaces/hyphens/commas |
| IMT `DestinationBankAccountName` | at least one letter, letters/digits/spaces/hyphens/apostrophes only, ≤62 characters |
| IMT `BeneficiaryAddress` | must not be blank, ≤40 characters after sanitization |
| IMT `PaymentReference` | must not be blank, ≤105 characters after sanitization |
| IMT batch size | 1–350 instructions |
| PP `Notes` | must not be blank |
| PP `PaymentDate` | today .. today + 14 months |
| PP `Amount` | must be > 0 |
| PP `DestinationBankBSB` | exactly 6 numeric digits |
| PP `DestinationBankAccountNo` | 3–9 characters |
| PP `DestinationBankAccountName` | no hyphen/apostrophe, ≤32 characters |
| PP `BeneficiaryAddress` | optional; if present, ≤40 characters, no hyphen/apostrophe |
| PP batch size | 1–350 instructions |
| FX `BuyCurrency`/`SellCurrency` | exactly 3 upper-case alphabetic characters |
| FX `Amount` | > 0, at most 11 integer digits + 2 decimal digits (max `99,999,999,999.99`) |
| FX `AccountNo` | 1–12 alphanumeric characters |
| FX batch size | 1–200 instructions |
| FX distinct currency pairs per batch | at most 15 |

---

## F-021 — Shared Field Mapping Model: verifying the `Mappings` field — TC-066–TC-070

> Added 2026-08-17 — this section did not exist at the runbook's original v1 writing; F-021 retrofitted
> the already-covered F-016/F-017 responses (and Direct Entry's) with a new field after the fact.

### What `Mappings` is and why it exists

Every successful `/convert` response now carries a second, parallel representation of the output
alongside `convertedText`: an ordered `mappings` array (`LineMapping[]`, one entry per output line/row),
where each line breaks down into a `fields` array (`FieldMapping[]`) of
`{ requestField, requestValue, cbaResponseField, cbaResponseValue }` tuples — see
[Features/Shared/FieldMapping.cs](../../src/CommBiz.Api/Features/Shared/FieldMapping.cs) (ADR-009). It
exists purely for tester/reviewer convenience: instead of manually parsing a fixed-width (Direct Entry)
or CSV (BPAY/IMT) `convertedText` line back apart field-by-field to check "did request field X end up in
the right place with the right value", `Mappings` states that mapping explicitly, in the same order the
line was written. It is strictly additive — never a replacement for `convertedText`, and never present
(`null`) whenever `success` is `false`.

**Automated equivalents:**
[tests/CommBiz.Api.Tests/DirectEntry/ConvertDirectEntryBatchHandlerTests.cs](../../tests/CommBiz.Api.Tests/DirectEntry/ConvertDirectEntryBatchHandlerTests.cs),
[tests/CommBiz.Api.Tests/BPay/ConvertBPayBatchHandlerTests.cs](../../tests/CommBiz.Api.Tests/BPay/ConvertBPayBatchHandlerTests.cs),
[tests/CommBiz.Api.Tests/Imt/ConvertImtBatchHandlerTests.cs](../../tests/CommBiz.Api.Tests/Imt/ConvertImtBatchHandlerTests.cs)

### TC-066 — Direct Entry: `Mappings` line order and header field attribution

POST the TC-056-style 2-instruction happy-path Direct Entry body (or reuse
[tests/smoke/DirectEntry.http](../../tests/smoke/DirectEntry.http)'s 2-instruction batch) and inspect
`mappings` instead of `convertedText`:

```powershell
$result = Invoke-RestMethod -Uri http://localhost:5182/convert -Method Post `
  -ContentType 'application/json' -Body $body

$result.mappings.line                      # expect: header, detail1, detail2, selfbalancing, trailer (in this order)
$detail1 = $result.mappings | Where-Object { $_.line -eq 'detail1' }
$detail1.fields | Format-Table requestField, requestValue, cbaResponseField, cbaResponseValue
```

**Expected:** `mappings` has exactly 5 lines, in order `header` → `detail1` → `detail2` → `selfbalancing`
→ `trailer` — matching `convertedText`'s own line order exactly. Spot-check 2–3 of `detail1.fields`
against the corresponding `\r\n`-split line of `convertedText` (e.g. the field whose `cbaResponseField`
is the amount should show the same digits that appear in that position of the raw detail line).

### TC-067 — BPay: `Mappings` line order and config-sourced header fields

POST the TC-056 2-instruction BPay body and inspect `mappings`:

```powershell
$result.mappings.line                      # expect: header, detail1, detail2 (no trailer, no self-balancing)
$header = $result.mappings | Where-Object { $_.line -eq 'header' }
$header.fields | Where-Object { $_.cbaResponseField -in @('File Number', 'Payment Account') } |
  Format-Table requestField, requestValue, cbaResponseField, cbaResponseValue
```

**Expected:** `mappings` has exactly 3 lines, in order `header` → `detail1` → `detail2` — no trailer, no
self-balancing (BPay has neither, per F-016 above). Within the `header` line, the `File Number` and
`Payment Account` entries' `requestField` is `FileNumber` / `FundingAccount` — the static
`appsettings.json` `BPay` settings field name — **not** any field from the request body, since those two
header values are config-sourced, not per-instruction.

### TC-068 — IMT: `Mappings` row order, no header/trailer lines

POST the TC-062 2-instruction IMT body and inspect `mappings`:

```powershell
$result.mappings.line                      # expect: row1, row2 (no header, no trailer)
$row2 = $result.mappings | Where-Object { $_.line -eq 'row2' }
$row2.fields | Format-Table requestField, requestValue, cbaResponseField, cbaResponseValue
```

**Expected:** `mappings` has exactly 2 lines, named `row1`/`row2` (not `header`/`detail*` — IMT has no
header or trailer record, per F-017 above). Spot-check `row2.fields` against `convertedText`'s second
CSV row (e.g. the field whose `cbaResponseField` is Beneficiary Address should show the sanitized
`"27 Oxford Street London W1D 2DZ"` value, matching TC-062).

### TC-069 — Edge case: single-instruction Direct Entry batch has no `detail2`

POST the TC-055 (or F-014) single-instruction Direct Entry body and inspect `mappings`:

```powershell
$result.mappings.line                      # expect: header, detail1, selfbalancing, trailer — no detail2
```

**Expected:** exactly 4 lines — `detail2` is entirely absent (not present-with-empty-fields), consistent
with `convertedText` splitting into 4 lines for a single-instruction batch (per F-014 above).

### TC-070 — Regression: a validation-failure response has `Mappings: null`

Reuse any Field validation failure body from this runbook (e.g. TC-058, BPay `AccountNo = ""`) or from
[phase-1-direct-entry-conversion-core.md](phase-1-direct-entry-conversion-core.md):

```powershell
$result = Invoke-RestMethod -Uri http://localhost:5182/convert -Method Post `
  -ContentType 'application/json' -Body $body

$result.success                            # expect: False
$result.mappings                           # expect: $null
```

**Expected:** `success: false` and `mappings: null` together, for every rejection scenario in this
runbook and in [phase-1-direct-entry-conversion-core.md](phase-1-direct-entry-conversion-core.md) —
`Mappings` is only ever populated alongside a successful conversion, never as a partial/best-effort
result on a rejected batch.

---

## F-018 — Priority Payments (RTGS) conversion — TC-071–TC-074

> Added 2026-08-18.

Priority Payments is routed on the API code `"RTGS"`; the CBA file's own Transaction Type field (field 1)
always writes the literal constant `"PP"`, and the file shares IMT's 27-field CSV row shape (no header or
trailer record, one row per instruction, CRLF-separated, no trailing CRLF after the last row). Unlike
IMT, Priority Payments is a **domestic, BSB-based** payment — there is no SWIFT code, IBAN, or
intermediary/correspondent bank at all; the destination is identified purely by a 6-digit BSB plus an
account number. Two further rules differ materially from IMT:

- **Process-date window:** Priority Payments allows `PaymentDate` up to **14 months** out, versus IMT's
  7-day window (see [Boundary/limit field reference](#boundarylimit-field-reference) above).
- **Character rules are stricter:** `DestinationBankAccountName` and (if present) `BeneficiaryAddress`
  must contain **no hyphen or apostrophe** at all. IMT's equivalent fields *allow* hyphens/apostrophes
  (see IMT's `DestinationBankAccountName` rule above) — Priority Payments does not.

The debit (source) account is **always** taken from the static `PriorityPaymentsSettings` configuration
(`appsettings.json`'s `PriorityPayments` section) — never from the request body; the request's own
`SourceBankAccountName`/`SourceBankAccountNo`/`SourceBankBsb` fields have since been removed from the
request DTO entirely, as they were never used to populate the debit side of the output.

**Automated equivalents:**
[tests/CommBiz.Api.Tests/PriorityPayments/PriorityPaymentValidatorTests.cs](../../tests/CommBiz.Api.Tests/PriorityPayments/PriorityPaymentValidatorTests.cs),
[tests/CommBiz.Api.Tests/PriorityPayments/PriorityPaymentRecordMapperTests.cs](../../tests/CommBiz.Api.Tests/PriorityPayments/PriorityPaymentRecordMapperTests.cs),
[tests/CommBiz.Api.Tests/PriorityPayments/ConvertPriorityPaymentBatchHandlerTests.cs](../../tests/CommBiz.Api.Tests/PriorityPayments/ConvertPriorityPaymentBatchHandlerTests.cs),
[tests/CommBiz.Api.Tests/PriorityPayments/PriorityPaymentConvertEndpointTests.cs](../../tests/CommBiz.Api.Tests/PriorityPayments/PriorityPaymentConvertEndpointTests.cs)

All scenarios below are drawn verbatim from
[tests/smoke/PriorityPayments.http](../../tests/smoke/PriorityPayments.http), which was verified live
against the running host during Integration.

### TC-071 — Happy path: single real sample instruction

From [tests/smoke/PriorityPayments.http](../../tests/smoke/PriorityPayments.http) (verbatim) — domestic
BSB-based instruction, destination BSB `012110`, `Amount: 10775`, `beneficiaryAddress` omitted (optional
field):

```powershell
$body = @'
[
  {
    "paymentTypeCode": "RTGS",
    "destinationBankAccountName": "ORS APP GATB",
    "destinationBankAccountNo": "838629371",
    "destinationBankBSB": "012110",
    "paymentDate": "2026-08-18T10:00:00",
    "amount": 10775.0,
    "notes": "Accounts has been paid to before.",
    "beneficiaryAddress": null
  }
]
'@

$result = Invoke-RestMethod -Uri http://localhost:5182/convert -Method Post `
  -ContentType 'application/json' -Body $body
```

**Expected:** `200 OK`, `success: true`, `errors: null`.

```powershell
$result.convertedText.StartsWith("PP,")     # expect: True — file literal, never "RTGS"
$result.convertedText.Split(',').Count      # expect: 27
$result.mappings.line                       # expect: row1 (only one entry — single-instruction batch)
$row1 = $result.mappings | Where-Object { $_.line -eq 'row1' }
$row1.fields.Count                          # expect: 27 — one FieldMapping entry per CSV field
```

### TC-072 — Happy path: 2 instructions, second with a populated `BeneficiaryAddress`

From [tests/smoke/PriorityPayments.http](../../tests/smoke/PriorityPayments.http) (verbatim) — second
instruction's `beneficiaryAddress` (`"9101 Alta Drive U15"`) contains letters/digits/spaces only, no
hyphen/apostrophe, per the stricter PP address rule:

```powershell
$result.convertedText -split "`r`n" | Measure-Object | Select-Object -ExpandProperty Count   # expect: 2
$result.mappings.line                        # expect: row1, row2
```

**Expected:** `200 OK`, `success: true`; two 27-field CSV rows joined by exactly one CRLF (no trailing
CRLF), `mappings` has 2 entries keyed `row1`/`row2`, each with 27 field entries.

### TC-073 — Edge case: batch mixing `RTGS` with another payment type is rejected

From [tests/smoke/PriorityPayments.http](../../tests/smoke/PriorityPayments.http) (verbatim) — a `DE`
instruction and an otherwise-valid `RTGS` instruction in the same batch:

```powershell
$body = @'
[
  {
    "paymentTypeCode": "DE",
    "accountNo": "S1605677",
    "paymentDate": "2026-08-20T10:00:00",
    "amount": 7500.0,
  },
  {
    "paymentTypeCode": "RTGS",
    "destinationBankAccountName": "ORS APP GATB",
    "destinationBankAccountNo": "838629371",
    "destinationBankBSB": "012110",
    "paymentDate": "2026-08-18T10:00:00",
    "amount": 10775.0,
    "notes": "Accounts has been paid to before.",
    "beneficiaryAddress": null
  }
]
'@

Invoke-RestMethod -Uri http://localhost:5182/convert -Method Post `
  -ContentType 'application/json' -Body $body
```

**Expected:** `200 OK`,
```json
{
  "success": false,
  "convertedText": null,
  "errors": [
    { "index": -1, "reason": "Payment batch must not mix payment types (found 'DE', 'RTGS')." }
  ]
}
```
The `RTGS` instruction is valid on its own (matches TC-071) — the rejection is purely about the mix,
consistent with `PaymentTypeRouter`'s batch-level behaviour (see F-015 above).

### TC-074 — Regression: malformed destination BSB returns `Mappings: null`

From [tests/smoke/PriorityPayments.http](../../tests/smoke/PriorityPayments.http) (verbatim) — TC-071's
body with `destinationBankBSB` blanked out:

```powershell
$body = @'
[
  {
    "paymentTypeCode": "RTGS",
    "destinationBankAccountName": "ORS APP GATB",
    "destinationBankAccountNo": "838629371",
    "destinationBankBSB": "",
    "paymentDate": "2026-08-18T10:00:00",
    "amount": 10775.0,
    "notes": "Accounts has been paid to before.",
    "beneficiaryAddress": null
  }
]
'@

$result = Invoke-RestMethod -Uri http://localhost:5182/convert -Method Post `
  -ContentType 'application/json' -Body $body
```

**Expected:**
```json
{
  "success": false,
  "convertedText": null,
  "errors": [
    { "index": 0, "reason": "DestinationBankBsb '' must be exactly 6 numeric digits." }
  ]
}
```
```powershell
$result.mappings                             # expect: $null
```
Consistent with F-021's TC-070 above — `Mappings` is only ever populated alongside a successful
conversion, never on a rejected batch.

---

## F-022 — Payment Type Router: FOREX dispatch — TC-075–TC-077

> Added 2026-08-18. **No `tests/smoke/Fx.http` file exists yet** (see [Prerequisites](#prerequisites)
> point 3 above) — every request body in this section and in F-023 below is written directly in this
> runbook, not lifted from a smoke file, unlike the F-015–F-021 sections above.

`PaymentTypeRouter` (F-015) gained a fifth routing code, `"FOREX"` (`FxType` in
[PaymentTypeRouter.cs](../../src/CommBiz.Api/Features/PaymentRouting/PaymentTypeRouter.cs)), dispatching
to the FX Conversion Slice (F-023, `Features/Fx`). The existing router-level rules apply unchanged: a
batch is peeked for `paymentTypeCode` before any slice's own validator runs, and a batch mixing `FOREX`
with any other payment type code is rejected in full at the router level — the same mixed-batch rule
already verified for `BPAY`/`DE`/`TT`/`RTGS` in F-015/TC-051 and F-018/TC-073 above.

**Automated equivalent:**
[tests/CommBiz.Api.Tests/Fx/FxConvertEndpointTests.cs](../../tests/CommBiz.Api.Tests/Fx/FxConvertEndpointTests.cs)

### TC-075 — Valid FOREX batch dispatches through `/convert` to the FX handler

```powershell
$body = @'
[
  {
    "paymentTypeCode": "FOREX",
    "paymentDate": "2026-08-18T10:00:00",
    "amount": 500.00,
    "notes": "New Settlement",
    "buyCurrency": "USD",
    "sellCurrency": "AUD",
    "rateTypeCode": "SPOT",
    "valueDateTypeCode": "STANDARD",
    "feeTypeCode": "OUR",
    "feeOtherTypeCode": "",
    "accountNo": "Payment2"
  }
]
'@

$result = Invoke-RestMethod -Uri http://localhost:5182/convert -Method Post `
  -ContentType 'application/json' -Body $body
```

**Expected:** `200 OK`, `success: true`, `errors: null`, `convertedText` starts with `"FX,"` (never
`"FOREX,"` — the routing code is never written to the file), `mappings` populated with 1 entry keyed
`row1`.

### TC-076 — Invalid FOREX batch returns failure with per-instruction reasons

Same body as TC-075 with `buyCurrency` shortened to `"US"` (2 letters, fails the FX slice's own
currency-code rule — see F-023/TC-082 below):

```powershell
$body = @'
[
  {
    "paymentTypeCode": "FOREX",
    "paymentDate": "2026-08-18T10:00:00",
    "amount": 500.00,
    "notes": "New Settlement",
    "buyCurrency": "US",
    "sellCurrency": "AUD",
    "rateTypeCode": "SPOT",
    "valueDateTypeCode": "STANDARD",
    "feeTypeCode": "OUR",
    "feeOtherTypeCode": "",
    "accountNo": "Payment2"
  }
]
'@

Invoke-RestMethod -Uri http://localhost:5182/convert -Method Post `
  -ContentType 'application/json' -Body $body
```

**Expected:** `200 OK`,
```json
{
  "success": false,
  "convertedText": null,
  "errors": [
    { "index": 0, "reason": "BuyCurrency 'US' must be exactly 3 uppercase alphabetic characters." }
  ]
}
```
`mappings: null` — the router successfully dispatched to the FX handler, but the FX slice's own
validator rejected the instruction; this is a slice-level rejection, not a router-level one (contrast
with TC-077 below).

### TC-077 — Batch mixing FOREX with another payment type is rejected in full

TC-075's valid instruction plus a minimal `DE` instruction in the same batch:

```powershell
$body = @'
[
  {
    "paymentTypeCode": "FOREX",
    "paymentDate": "2026-08-18T10:00:00",
    "amount": 500.00,
    "notes": "New Settlement",
    "buyCurrency": "USD",
    "sellCurrency": "AUD",
    "rateTypeCode": "SPOT",
    "valueDateTypeCode": "STANDARD",
    "feeTypeCode": "OUR",
    "feeOtherTypeCode": "",
    "accountNo": "Payment2"
  },
  {
    "paymentTypeCode": "DE",
  }
]
'@

Invoke-RestMethod -Uri http://localhost:5182/convert -Method Post `
  -ContentType 'application/json' -Body $body
```

**Expected:** `200 OK`,
```json
{
  "success": false,
  "convertedText": null,
  "errors": [
    { "index": -1, "reason": "Payment batch must not mix payment types (found 'FOREX', 'DE')." }
  ]
}
```
This is a **router-level** rejection (`index: -1`) — the batch never reaches either slice's own
validator, exactly like F-015/TC-051 and F-018/TC-073 above; `FOREX` simply extends the same
already-existing mixed-batch rule to a fifth type code.

---

## F-023 — FX (Foreign Exchange) conversion — TC-078–TC-084

The FX file is CSV, comma-delimited, 27 fields, one row per instruction, CRLF-separated with **no
header, no trailer, and no trailing CRLF after the last row** — the same shape as IMT and Priority
Payments. Field 1 (Transaction Type) always writes the literal constant `"FX"` — never the API routing
code `"FOREX"`. Only the IPFX spec's non-CBA "Instruction" pattern (Sample 2: I SELL Instruction =
`MAN`, I BUY Instruction = `DOC`) is supported; the IDR/CNH/KRW conditional fields (positions 23–27) are
deferred per [docs/architecture.md](../architecture.md) Open Question A6, and are always blank.

Per [FxRecordMapper.cs](../../src/CommBiz.Api/Features/Fx/FxRecordMapper.cs), of the 27 fields:

| Field # | Name | Source |
|---|---|---|
| 1 | Transaction Type | constant `"FX"` |
| 2 | Transaction Description | request `AccountNo` |
| 3 | I BUY Currency | request `BuyCurrency` |
| 4 | I BUY Amount | blank — amount is always placed on the **Sell** side |
| 5 | I SELL Currency | request `SellCurrency` |
| 6 | I SELL Amount | request `Amount` |
| 7 | I SELL Instruction | config `Fx:SellInstruction` (confirmed value `"MAN"`) |
| 12 | I BUY Instruction | config `Fx:BuyInstruction` (confirmed value `"DOC"`) |
| 21 | I BUY Payment details | config `Fx:BuyPaymentDetails` (confirmed value `"Buy"`) |
| 22 | I SELL Payment details | config `Fx:SellPaymentDetails` (confirmed value `"Sell"`) |
| 8, 9, 10, 11, 13–20, 23–27 | (18 remaining positions) | always blank |

**Automated equivalents:**
[tests/CommBiz.Api.Tests/Fx/FxValidatorTests.cs](../../tests/CommBiz.Api.Tests/Fx/FxValidatorTests.cs),
[tests/CommBiz.Api.Tests/Fx/FxRecordMapperTests.cs](../../tests/CommBiz.Api.Tests/Fx/FxRecordMapperTests.cs),
[tests/CommBiz.Api.Tests/Fx/ConvertFxBatchHandlerTests.cs](../../tests/CommBiz.Api.Tests/Fx/ConvertFxBatchHandlerTests.cs),
[tests/CommBiz.Api.Tests/Fx/FxConvertEndpointTests.cs](../../tests/CommBiz.Api.Tests/Fx/FxConvertEndpointTests.cs)

### TC-078 — Happy path: single instruction, full field mapping

Reuse TC-075's body (`buyCurrency: "USD"`, `sellCurrency: "AUD"`, `amount: 500.00`,
`accountNo: "Payment2"`):

```powershell
$fields = $result.convertedText.Split(',')
$fields.Count            # expect: 27
$fields[0]                # "FX"        (field 1)
$fields[1]                # "Payment2"  (field 2: Transaction Description, from AccountNo)
$fields[2]                # "USD"       (field 3: I BUY Currency)
$fields[3]                # ""          (field 4: I BUY Amount — always blank)
$fields[4]                # "AUD"       (field 5: I SELL Currency)
$fields[5]                # "500.00"    (field 6: I SELL Amount, from Amount)
$fields[6]                # "MAN"       (field 7: I SELL Instruction)
$fields[11]               # "DOC"       (field 12: I BUY Instruction)
$fields[20]                # "Buy"       (field 21: I BUY Payment details)
$fields[21]                # "Sell"      (field 22: I SELL Payment details)
```

**Expected:** `200 OK`, `success: true`, `errors: null`; every field above matches; all 18 remaining
positions (8, 9, 10, 11, 13–20, 23–27) are empty strings.

### TC-079 — Happy path: 2 instructions, distinct currency pairs — `Mappings` row order/count

Same as TC-078 plus a second instruction with a different currency pair (`GBP`/`AUD`):

```powershell
$body = @'
[
  {
    "paymentTypeCode": "FOREX",
    "paymentDate": "2026-08-18T10:00:00",
    "amount": 500.00,
    "notes": "New Settlement",
    "buyCurrency": "USD",
    "sellCurrency": "AUD",
    "rateTypeCode": "SPOT",
    "valueDateTypeCode": "STANDARD",
    "feeTypeCode": "OUR",
    "feeOtherTypeCode": "",
    "accountNo": "Payment2"
  },
  {
    "paymentTypeCode": "FOREX",
    "paymentDate": "2026-08-18T10:00:00",
    "amount": 1250.75,
    "notes": "New Settlement",
    "buyCurrency": "GBP",
    "sellCurrency": "AUD",
    "rateTypeCode": "SPOT",
    "valueDateTypeCode": "STANDARD",
    "feeTypeCode": "OUR",
    "feeOtherTypeCode": "",
    "accountNo": "Payment3"
  }
]
'@

$result = Invoke-RestMethod -Uri http://localhost:5182/convert -Method Post `
  -ContentType 'application/json' -Body $body

$result.convertedText -split "`r`n" | Measure-Object | Select-Object -ExpandProperty Count   # expect: 2
$result.mappings.line                        # expect: row1, row2 (no header, no trailer)
$row2 = $result.mappings | Where-Object { $_.line -eq 'row2' }
$row2.fields.Count                           # expect: 27 — one FieldMapping entry per CSV field
```

**Expected:** `200 OK`, `success: true`; two 27-field CSV rows joined by exactly one CRLF, no trailing
CRLF; `mappings` has 2 entries keyed `row1`/`row2`, each with 27 field entries — matching the
`convertedText`/`Mappings` shape already verified for IMT/PP in F-021/TC-068 above.

### TC-080 — Edge case: batch-count boundaries (0 / 1 / 200 / 201)

All four variants below use a single, fixed currency pair (`USD`/`AUD`) throughout, so the batch-count
rule is isolated from the currency-pair-count rule (F-023/TC-081 below).

| Instruction count | Body shape | Expected |
|---|---|---|
| 0 | `[]` (empty array) | **Router-level** rejection (F-015 rule) — `200 OK`, `"success": false`, `errors: [{ "index": -1, "reason": "Payment batch must contain at least 1 payment instruction; unable to determine payment type." }]`. The FX slice's own `MinimumInstructionCount` check in `FxValidator` is never reached, since `PaymentTypeRouter` rejects empty arrays before any `paymentTypeCode` can be read. |
| 1 | TC-078's single instruction | `200 OK`, `success: true` (already covered above). |
| 200 | 200 instructions, same currency pair, unique `accountNo` per instruction (e.g. `"FX000001"`..`"FX000200"`) | `200 OK`, `success: true` — the maximum boundary is inclusive. |
| 201 | 201 instructions, otherwise identical to the 200 case | `200 OK`, `"success": false`, `errors: [{ "index": -1, "reason": "FX file must contain at most 200 payment instruction(s) (found 201)." }]` |

Generate the 200/201-instruction bodies with a small script rather than hand-writing them — e.g.:

```powershell
function New-FxBatch([int]$count) {
    1..$count | ForEach-Object {
        [PSCustomObject]@{
            paymentTypeCode  = "FOREX"
            paymentDate      = "2026-08-18T10:00:00"
            amount           = 500.00
            notes            = "New Settlement"
            buyCurrency      = "USD"
            sellCurrency     = "AUD"
            rateTypeCode     = "SPOT"
            valueDateTypeCode = "STANDARD"
            feeTypeCode      = "OUR"
            feeOtherTypeCode = ""
            accountNo        = "FX{0:D6}" -f $_
        }
    }
}

$batch200 = New-FxBatch -count 200 | ConvertTo-Json
Invoke-RestMethod -Uri http://localhost:5182/convert -Method Post `
  -ContentType 'application/json' -Body $batch200

$batch201 = New-FxBatch -count 201 | ConvertTo-Json
Invoke-RestMethod -Uri http://localhost:5182/convert -Method Post `
  -ContentType 'application/json' -Body $batch201
```

### TC-081 — Edge case: currency-pair-count boundary (15 / 16 distinct pairs)

Both variants below use a small, fixed instruction count (one instruction per distinct pair, well under
the 200-instruction batch-count limit), so this boundary is isolated from TC-080 above. Fifteen distinct
`BuyCurrency` values against a constant `SellCurrency` of `AUD` gives 15 distinct pairs; adding a 16th
buy currency tips it over:

```powershell
function New-FxCurrencyPairBatch([string[]]$buyCurrencies) {
    $buyCurrencies | ForEach-Object {
        [PSCustomObject]@{
            paymentTypeCode  = "FOREX"
            paymentDate      = "2026-08-18T10:00:00"
            amount           = 500.00
            notes            = "New Settlement"
            buyCurrency      = $_
            sellCurrency     = "AUD"
            rateTypeCode     = "SPOT"
            valueDateTypeCode = "STANDARD"
            feeTypeCode      = "OUR"
            feeOtherTypeCode = ""
            accountNo        = "FX-$_"
        }
    }
}

$fifteenPairs = @("USD","GBP","EUR","JPY","NZD","CAD","CHF","SGD","HKD","CNY","INR","ZAR","SEK","NOK","DKK")
$batch15 = New-FxCurrencyPairBatch -buyCurrencies $fifteenPairs | ConvertTo-Json
Invoke-RestMethod -Uri http://localhost:5182/convert -Method Post `
  -ContentType 'application/json' -Body $batch15
# Expect: 200 OK, success: true

$sixteenPairs = $fifteenPairs + "THB"
$batch16 = New-FxCurrencyPairBatch -buyCurrencies $sixteenPairs | ConvertTo-Json
Invoke-RestMethod -Uri http://localhost:5182/convert -Method Post `
  -ContentType 'application/json' -Body $batch16
```

**Expected (16-pair batch):** `200 OK`,
```json
{
  "success": false,
  "convertedText": null,
  "errors": [
    { "index": -1, "reason": "FX file must settle at most 15 distinct currency pairs (found 16)." }
  ]
}
```

### TC-082/TC-083/TC-084 — Field validation failures

Start from TC-078's single-instruction body and apply only the described mutation to one field, then
POST. All return `200 OK` with `"success": false`, `"convertedText": null`, `"mappings": null`.

| TC | Mutation | Expected `errors[0].reason` | `index` |
|---|---|---|---|
| TC-082 | `buyCurrency = "US"` (2 letters) | `BuyCurrency 'US' must be exactly 3 uppercase alphabetic characters.` | `0` |
| TC-083 | `amount = 100.123` (3 decimal digits) | `Amount '100.123' must be greater than zero, with at most 11 integer digits and 2 decimal digits.` | `0` |
| TC-084 | `accountNo = "ABCDEFGHIJKLM"` (13 alphanumeric characters, exceeds the 12-character max) | `AccountNo 'ABCDEFGHIJKLM' must be 1-12 alphanumeric characters.` | `0` |

```powershell
$body = @'
[
  {
    "paymentTypeCode": "FOREX",
    "paymentDate": "2026-08-18T10:00:00",
    "amount": 500.00,
    "notes": "New Settlement",
    "buyCurrency": "US",
    "sellCurrency": "AUD",
    "rateTypeCode": "SPOT",
    "valueDateTypeCode": "STANDARD",
    "feeTypeCode": "OUR",
    "feeOtherTypeCode": "",
    "accountNo": "Payment2"
  }
]
'@

Invoke-RestMethod -Uri http://localhost:5182/convert -Method Post `
  -ContentType 'application/json' -Body $body
```

**Expected (TC-082):**
```json
{
  "success": false,
  "convertedText": null,
  "errors": [
    { "index": 0, "reason": "BuyCurrency 'US' must be exactly 3 uppercase alphabetic characters." }
  ]
}
```
