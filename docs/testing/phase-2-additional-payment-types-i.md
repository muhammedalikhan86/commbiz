# Test Runbook: Phase 2 — Additional Payment Types I

> Status: DRAFT
> Version: v1
> Last updated: 2026-08-17
> Covers: F-015–F-017 (see [docs/project-management.md](../project-management.md))
> Scenario reference: [docs/test-cases.md](../test-cases.md) TC-051–TC-065 (independently numbered
> range, disjoint from test-cases.md's own TC-033–TC-050 Phase 2 section — some scenarios below are
> manual walkthroughs of the same underlying rules test-cases.md covers, e.g. TC-051/TC-052 here mirror
> test-cases.md's TC-004/TC-005 mixed-type/unsupported-type rejections)
> Source data: [docs/stash/BPay Payments - CommBiz File Specification.md](../stash/BPay%20Payments%20-%20CommBiz%20File%20Specification.md) §1.1/§1.3,
> [docs/stash/CommBiz File Specification - International Money Transfers Priority Payments Non CBA Payment Requests (MT101) v9.md](../stash/CommBiz%20File%20Specification%20-%20International%20Money%20Transfers%20Priority%20Payments%20Non%20CBA%20Payment%20Requests%20%28MT101%29%20v9.md) §1.2/§1.4

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
   [tests/smoke/Imt.http](../../tests/smoke/Imt.http), and
   [tests/smoke/Errors.http](../../tests/smoke/Errors.http).
4. `appsettings.json`'s `BPay` section (`FundingAccount`, `FileNumber`) uses placeholder values only —
   no real funding account has been confirmed yet (per [Features/BPay/README.md](../../src/CommBiz.Api/Features/BPay/README.md)).
   `appsettings.json`'s `Imt` section (`DebitAccountBsb`, `DebitAccountNumber`, `DebitAccountName`) uses
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
    "PaymentSourceTypeCode": "LEDGER",
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
    "paymentSourceTypeCode": "LEDGER",
    "sourceBankAccountName": null,
    "sourceBankAccountNo": null,
    "sourceBankBSB": null,
    "destinationBankTypeCode": null,
    "destinationBankAccountName": "SAMER MOHAMMED KIKI",
    "destinationBankAccountNo": "658450191",
    "paymentDate": "2026-08-18T10:00:00",
    "sourceCurrency": "USD",
    "sourceAmount": 588517.58,
    "amount": 0.0,
    "paymentReference": "TT-000001",
    "notes": "10 bps on fx",
    "currency": null,
    "destinationBankIBAN": null,
    "destinationBankSWIFTCode": "CHASUS33",
    "destinationBankName": "NATIONAL FINANCIAL SERVICES",
    "destinationBankAddress": "640 5TH AVENUE NEW YORK NY 10019",
    "beneficiaryAddress": "9101 Alta Drive, U15, Las Vegas, NV 89145",
    "intermediaryBankIBAN": null,
    "intermediaryBankSWIFTCode": "CHASUS33",
    "intermediaryBankName": "CHASE MANHATTAN BANK",
    "intermediaryBankAddress": "1 CHASE MANHATTAN PLAZA NEW YORK NY 10005"
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

### TC-056 — Happy path: 2 valid instructions

From [tests/smoke/BPay.http](../../tests/smoke/BPay.http) (verbatim):

```powershell
$body = @'
[
  {
    "PaymentTypeCode": "BPAY",
    "AccountNo": "S1218945",
    "PaymentSourceTypeCode": "LEDGER",
    "PaymentDate": "2026-08-11T12:00:00",
    "Amount": 625.90,
    "BPayBillerCode": "488577",
    "BPayReference": "1202194308172126"
  },
  {
    "PaymentTypeCode": "BPAY",
    "AccountNo": "S1218946",
    "PaymentSourceTypeCode": "LEDGER",
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
    "PaymentSourceTypeCode": "LEDGER",
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
    "PaymentSourceTypeCode": "LEDGER",
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
    "paymentSourceTypeCode": "LEDGER",
    "sourceBankAccountName": null,
    "sourceBankAccountNo": null,
    "sourceBankBSB": null,
    "destinationBankTypeCode": null,
    "destinationBankAccountName": "SAMER MOHAMMED KIKI",
    "destinationBankAccountNo": "658450191",
    "paymentDate": "2026-08-18T10:00:00",
    "sourceCurrency": "USD",
    "sourceAmount": 588517.58,
    "amount": 0.0,
    "paymentReference": "TT-000001",
    "notes": "10 bps on fx",
    "currency": null,
    "destinationBankIBAN": null,
    "destinationBankSWIFTCode": "CHASUS33",
    "destinationBankName": "NATIONAL FINANCIAL SERVICES",
    "destinationBankAddress": "640 5TH AVENUE NEW YORK NY 10019",
    "beneficiaryAddress": "9101 Alta Drive, U15, Las Vegas, NV 89145",
    "intermediaryBankIBAN": null,
    "intermediaryBankSWIFTCode": "CHASUS33",
    "intermediaryBankName": "CHASE MANHATTAN BANK",
    "intermediaryBankAddress": "1 CHASE MANHATTAN PLAZA NEW YORK NY 10005"
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
    "paymentSourceTypeCode": "LEDGER",
    "sourceBankAccountName": null,
    "sourceBankAccountNo": null,
    "sourceBankBSB": null,
    "destinationBankTypeCode": null,
    "destinationBankAccountName": "SAMER MOHAMMED KIKI",
    "destinationBankAccountNo": "658450191",
    "paymentDate": "2026-08-18T10:00:00",
    "sourceCurrency": "USD",
    "sourceAmount": 588517.58,
    "amount": 0.0,
    "paymentReference": "TT-000001",
    "notes": "10 bps on fx",
    "currency": null,
    "destinationBankIBAN": null,
    "destinationBankSWIFTCode": "CHASUS33",
    "destinationBankName": "NATIONAL FINANCIAL SERVICES",
    "destinationBankAddress": "640 5TH AVENUE NEW YORK NY 10019",
    "beneficiaryAddress": "9101 Alta Drive, U 15, Las Vegas, NV 89145",
    "intermediaryBankIBAN": null,
    "intermediaryBankSWIFTCode": "CHASUS33",
    "intermediaryBankName": "CHASE MANHATTAN BANK",
    "intermediaryBankAddress": "1 CHASE MANHATTAN PLAZA NEW YORK NY 10005"
  },
  {
    "paymentTypeCode": "TT",
    "paymentSourceTypeCode": "LEDGER",
    "sourceBankAccountName": null,
    "sourceBankAccountNo": null,
    "sourceBankBSB": null,
    "destinationBankTypeCode": null,
    "destinationBankAccountName": "GLOBEX TRADING PTY LTD",
    "destinationBankAccountNo": "4471820033",
    "paymentDate": "2026-08-18T10:00:00",
    "sourceCurrency": "GBP",
    "sourceAmount": 42500.00,
    "amount": 0.0,
    "paymentReference": "TT-000002",
    "notes": "Supplier invoice settlement",
    "currency": null,
    "destinationBankIBAN": null,
    "destinationBankSWIFTCode": "BARCGB22",
    "destinationBankName": "BARCLAYS BANK UK PLC",
    "destinationBankAddress": "1 CHURCHILL PLACE LONDON E14 5HP",
    "beneficiaryAddress": "27 Oxford Street, London W1D 2DZ",
    "intermediaryBankIBAN": null,
    "intermediaryBankSWIFTCode": null,
    "intermediaryBankName": null,
    "intermediaryBankAddress": null
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
    "paymentSourceTypeCode": "LEDGER",
    "sourceBankAccountName": null,
    "sourceBankAccountNo": null,
    "sourceBankBSB": null,
    "destinationBankTypeCode": null,
    "destinationBankAccountName": "SAMER MOHAMMED KIKI",
    "destinationBankAccountNo": "658450191",
    "paymentDate": "2026-08-18T10:00:00",
    "sourceCurrency": "USD",
    "sourceAmount": 588517.58,
    "amount": 0.0,
    "paymentReference": "TT-000001",
    "notes": "",
    "currency": null,
    "destinationBankIBAN": null,
    "destinationBankSWIFTCode": "CHASUS33",
    "destinationBankName": "NATIONAL FINANCIAL SERVICES",
    "destinationBankAddress": "640 5TH AVENUE NEW YORK NY 10019",
    "beneficiaryAddress": "9101 Alta Drive, U15, Las Vegas, NV 89145",
    "intermediaryBankIBAN": null,
    "intermediaryBankSWIFTCode": "CHASUS33",
    "intermediaryBankName": "CHASE MANHATTAN BANK (J.P. MORGAN CHASE & CO)",
    "intermediaryBankAddress": "1 CHASE MANHATTAN PLAZA NEW YORK NY 10005"
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
