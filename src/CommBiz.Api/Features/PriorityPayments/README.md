# Priority Payments Slice

The Priority Payments vertical slice (ADR-002, F-018) — also known as RTGS, Shaw and Partners' own
name for this payment type; the router recognises the API's `"RTGS"` code, but every row's own
Transaction Type field (field 1) always writes the literal constant `"PP"`, per the CBA spec — the
same naming-distinction pattern as IMT's `"TT"` → `"IMT"`. Owns its own request model, validation,
mapping, and output-assembly logic for converting Priority Payments batches end to end, per
`docs/stash/CommBiz File Specification - International Money Transfers Priority Payments Non CBA
Payment Requests (MT101) v9.md` §1.2/§1.5: request/response contract, field validation
(`PriorityPaymentValidator`), 27-field CSV row mapping (`PriorityPaymentRecordMapper`), and file
assembly (`ConvertPriorityPaymentBatchCommand`/`ConvertPriorityPaymentBatchHandler`). Dispatched to
from the top-level Payment Type Router (`Features/PaymentRouting`, F-015).

Shares IMT's 27-field MT101-family CSV format, but is a domestic, BSB-based payment: almost every
SWIFT/currency/intermediary field is "Not applicable" (always blank). Key differences from IMT:
- 14-month process-date window (vs. IMT's 7 days)
- Stricter beneficiary name/address character rules — letters, digits, and spaces only, no
  hyphen/apostrophe (IMT permits both)
- Beneficiary Bank BSB (field 14) is a plain 6-digit number, not hyphenated like Direct Entry's
  `nnn-nnn`
- Debit account (field 7) is derived entirely from the static `PriorityPaymentsSettings`
  configuration, never from the request — `SourceBankBsb`/`SourceBankAccountNo` are carried by the
  payload but unused, same treatment IMT gives its own unused fields

Like IMT, the file has **no header or trailer record** — it is simply one 27-field CSV row per
instruction, CRLF-separated, with no trailing CRLF after the last row.

`appsettings.json`'s `PriorityPayments` section (`DebitAccountBsb`, `DebitAccountNumber`,
`DebitAccountName`) uses real, confirmed values (same 062-000/2112 0075 settlement account IMT
already uses), not placeholders.

## Field Mapping

Where each CBA field comes from: `request.*` (dynamic, per-instruction), `appsetting.*`
(`PriorityPaymentsSettings`, static config), or `constants.*` (hardcoded in the mapper). Fields not
applicable to a domestic Priority Payment are always blank. Position is the field's 1-based index in
the comma-separated CSV row (27 fields total — no header/trailer record).

### Payment Row (`PriorityPaymentRecordMapper`)

| CBA Field | Position | Source | Transformation |
|---|---|---|---|
| Transaction Type | 1 | constants.TransactionType | none (always `PP`, never the API's `RTGS` routing code) |
| Transaction Description | 2 | request.Notes | truncated to 12 characters |
| Process Date | 3 | request.PaymentDate | formatted `yyMMdd` |
| Payment Currency | 4 | not applicable | empty |
| Payment Amount | 5 | request.Amount | none |
| Debit Amount | 6 | not applicable | empty |
| Debit Account - Account Number | 7 | appsetting.DebitAccountBsb + appsetting.DebitAccountNumber (aggregate) | last 4 BSB digits + account number, hyphens/spaces stripped |
| Dealer Code | 8 | not applicable | empty |
| Dealer Exchange Rate | 9 | not applicable | empty |
| Intermediary Bank - Bank Code | 10 | not applicable | empty |
| Intermediary Bank - Name | 11 | not applicable | empty |
| Intermediary Bank - City | 12 | not applicable | empty |
| Intermediary Institution - Country | 13 | not applicable | empty |
| Beneficiary Bank - Bank Code | 14 | request.DestinationBankBsb | none (plain 6-digit BSB, not hyphenated) |
| Beneficiary Bank - Name | 15 | not applicable | empty |
| Beneficiary Bank - City | 16 | not applicable | empty |
| Beneficiary Bank - Country | 17 | not applicable | empty |
| Beneficiary - Account Number | 18 | request.DestinationBankAccountNo | none |
| Beneficiary - Account Name | 19 | request.DestinationBankAccountName | none |
| Beneficiary - Address line 1 | 20 | request.BeneficiaryAddress | sanitized: disallowed characters replaced with a space, repeated spaces collapsed |
| Beneficiary - Address line 2 | 21 | not applicable | empty |
| Beneficiary - Address line 3 | 22 | not applicable | empty |
| Beneficiary - City | 23 | not applicable | empty |
| Beneficiary - State | 24 | not applicable | empty |
| Beneficiary - Postcode | 25 | not applicable | empty |
| Beneficiary - Country Code | 26 | not applicable | empty |
| Beneficiary Payment Details | 27 | not applicable | empty (Notes already maps to field 2) |

## Exception List (Non-Negotiables)

Conditions checked by `PriorityPaymentValidator` that cause the whole batch to be thrown back
(rejected) rather than converted. Any one of these, on any single instruction, rejects the entire
file - no partial conversion.

| Field | Non-negotiable rule |
|---|---|
| Batch size | 1-350 transactions per file (shared IMT/PP/NonCBA limit) |
| Notes | must not be blank |
| PaymentDate | between today and 14 months ahead |
| Amount | positive, at most 11 integer + 2 decimal digits (≤ 99,999,999,999.99) |
| DestinationBankBsb | exactly 6 numeric digits |
| DestinationBankAccountNo | 3-9 alphanumeric characters |
| DestinationBankAccountName | must not be blank, letters/digits/spaces only, at most 32 characters |
| BeneficiaryAddress (optional) | if present: at most 40 characters, letters/digits/spaces only |

## Sanitisation (Pre-Validation)

Priority Payments has **no pre-validation sanitisation step** - there is no `Sanitize`-style method
run before `PriorityPaymentValidator.Validate`.

`PriorityPaymentRecordMapper.SanitizeAddress` (replace disallowed characters with a space, collapse
repeated spaces) runs only in the mapping layer, *after* validation. In practice its
character-replacement behaviour is unreachable: `PriorityPaymentValidator` already rejects any
address containing a character outside letters/digits/spaces, so by the time `SanitizeAddress` runs
there are no disallowed characters left to replace. The only part of `SanitizeAddress` that has any
real effect is collapsing repeated spaces, which validation permits (space is a valid character) but
which still changes the value between what was validated and what is written to the file.

## Pattern Compliance (request → sanitisation → validation → map)

⚠️ **Minor violation (low severity, likely acceptable).** `BeneficiaryAddress` is validated in its
raw form (which may contain repeated spaces) and then cosmetically altered in the map step - so the
exact bytes validated and the exact bytes written can differ, which breaks "validate what you
actually write." Unlike Direct Entry/Fx's gaps, this can't let an otherwise-invalid value slip
through (the character-set check already ran), so the practical risk is limited to whitespace
cosmetics. Recommend moving the space-collapse into an explicit pre-validation sanitise step (same
shape as `ImtValidator.Sanitize`) purely for consistency, not because of any known defect.
