# IMT Slice

The International Money Transfers (IMT) vertical slice (ADR-002, F-017). Owns its own request
model, validation, mapping, and output-assembly logic for converting IMT/MT101-family payment
batches end to end, per `docs/stash/CommBiz File Specification - International Money Transfers
Priority Payments Non CBA Payment Requests (MT101) v9.md`: request/response contract, field
validation (`ImtValidator`), 27-field CSV row mapping (`ImtRecordMapper`), and file assembly
(`ConvertImtBatchCommand`/`ConvertImtBatchHandler`). Dispatched to from the top-level Payment Type
Router (`Features/PaymentRouting`) on the API's `"TT"` payment type code (F-015).

**Naming distinction:** the API's routing code is `"TT"` ("Telegraphic Transfer" - Shaw and
Partners' internal name), but every row's Transaction Type field (field 1) always writes the
literal constant `"IMT"`, per the CBA file spec - the router recognizes `"TT"`, the file never does.

Unlike Direct Entry and BPAY, the IMT file has **no header or trailer record** - it is simply one
27-field CSV row per instruction, CRLF-separated, with **no trailing CRLF after the last row**
(§1.2 rule 2) - this is the one place this slice's assembly rule differs from the other two, and is
called out explicitly in `ConvertImtBatchHandler` so a future refactor doesn't "fix" it to match.

Fields 13/17/26 (country codes) are derived from characters 5-6 of the relevant SWIFT BIC rather
than supplied directly by the request - there is no discrete country field in the payload. Field 7
(the debit account number) is derived entirely from the static `ImtSettings` configuration (BSB
last-4 + account number), never from the request, mirroring Direct Entry's
`TraceAccountBsb`/`TraceAccountAccNo` and BPay's confirmed `FundingAccount` pattern.

**Reject-vs-sanitize split (per the confirmed field mapping):** Beneficiary Account Name, Beneficiary
Bank Name, and SWIFT codes are rejected outright on invalid characters or overlong values - never
silently altered. Beneficiary Payment Details is sanitized (disallowed characters, including commas,
replaced with a space), then still rejected if the sanitized value exceeds its length limit.
Beneficiary Account Number (separators stripped), Beneficiary Address, and Intermediary Bank Name are
sanitized *and* truncated rather than rejected - see `ImtValidator.Sanitize`, which runs before
`Validate` so these three fields can never fail validation on content or length.

`appsettings.json`'s `Imt` section (`DebitAccountBsb`, `DebitAccountNumber`, `DebitAccountName`)
uses real, confirmed values (F-017), not placeholders.

## Field Mapping

Where each CBA field comes from: `request.*` (dynamic, per-instruction), `appsetting.*`
(`ImtSettings`, static config), or `constants.*` (hardcoded in the mapper). Position is the field's
1-based index in the comma-separated CSV row (27 fields total - no header/trailer record).
`ImtValidator.Sanitize` runs before validation/mapping and mutates `DestinationBankAccountNo`,
`BeneficiaryAddress`, and `IntermediaryBankName` in place - the "Transformation" column below
reflects what `ImtRecordMapper` itself still does to the (by then already-sanitized) value.

### Payment Row (`ImtRecordMapper`)

| CBA Field | Position | Source | Transformation |
|---|---|---|---|
| Transaction Type | 1 | constants.TransactionType | none (always `IMT`, never the API's `TT` routing code) |
| Transaction Description | 2 | request.Notes | truncated to 12 characters |
| Process Date | 3 | request.PaymentDate | formatted `yyMMdd` |
| Payment Currency | 4 | request.SourceCurrency | none |
| Payment Amount | 5 | request.SourceAmount | blank unless `SourceAmount > 0` |
| Debit Amount | 6 | request.Amount | blank unless `Amount > 0` |
| Debit Account - Account Number | 7 | appsetting.DebitAccountBsb + appsetting.DebitAccountNumber (aggregate) | last 4 BSB digits + account number, hyphens/spaces stripped |
| Dealer Code | 8 | not present in payload | empty |
| Dealer Exchange Rate | 9 | not present in payload | empty |
| Intermediary Bank - Bank Code | 10 | request.IntermediaryBankSwiftCode | none |
| Intermediary Bank - Name | 11 | request.IntermediaryBankName | sanitized (letters/digits/spaces/hyphens/apostrophes only) and truncated to 30 characters, by `ImtValidator.Sanitize` |
| Intermediary Bank - City | 12 | no discrete city field | empty |
| Intermediary Institution - Country | 13 | request.IntermediaryBankSwiftCode (aggregate) | derived: characters 5-6 of the SWIFT BIC |
| Beneficiary Bank - Bank Code | 14 | request.DestinationBankSwiftCode | none |
| Beneficiary Bank - Name | 15 | request.DestinationBankName | none (rejected outright, not sanitized, if blank or over 30 characters) |
| Beneficiary Bank - City | 16 | no discrete city field | empty |
| Beneficiary Bank - Country | 17 | request.DestinationBankSwiftCode (aggregate) | derived: characters 5-6 of the SWIFT BIC |
| Beneficiary - Account Number | 18 | request.DestinationBankAccountNo | spaces/hyphens/commas stripped, by `ImtValidator.Sanitize` |
| Beneficiary - Account Name | 19 | request.DestinationBankAccountName | none (rejected outright, not sanitized, on invalid characters or length) |
| Beneficiary - Address line 1 | 20 | request.BeneficiaryAddress | sanitized (letters/digits/spaces/hyphens/apostrophes only) and truncated to 40 characters, by `ImtValidator.Sanitize` |
| Reserved For Future Use | 21 | never populated - rejected by CommBiz otherwise | empty |
| Reserved For Future Use | 22 | never populated - rejected by CommBiz otherwise | empty |
| Beneficiary - City | 23 | no discrete field available | empty |
| Beneficiary - State | 24 | no discrete field available | empty |
| Beneficiary - Postcode | 25 | no discrete field available | empty |
| Beneficiary - Country | 26 | request.DestinationBankSwiftCode (aggregate) | derived: same as field 17 |
| Beneficiary Payment Details | 27 | request.PaymentReference | sanitized (disallowed characters replaced with a space); still rejected if the sanitized value exceeds 105 characters |

## Exception List (Non-Negotiables)

Conditions checked by `ImtValidator.Validate` (running on the already-sanitised instruction - see
Sanitisation below) that cause the whole batch to be thrown back (rejected) rather than converted.
Any one of these, on any single instruction, rejects the entire file - no partial conversion.

| Field | Non-negotiable rule |
|---|---|
| Batch size | 1-350 transactions per file |
| Notes | must not be blank |
| PaymentDate | between today and today + 7 days |
| SourceCurrency | exactly 3 upper-case letters |
| SourceAmount / Amount | exactly one of the two must be > 0, and the populated one must be positive with at most 11 integer + 2 decimal digits |
| IntermediaryBankSwiftCode (if provided) | 8 or 11 alphanumeric characters |
| DestinationBankSwiftCode | 8 or 11 alphanumeric characters |
| DestinationBankName | must not be blank, at most 30 characters - rejected outright, never sanitised |
| DestinationBankAccountNo (post-sanitisation) | 1-34 characters |
| DestinationBankAccountName | must contain at least one letter, letters/digits/spaces/hyphens/apostrophes only, at most 62 characters - rejected outright, never sanitised |
| BeneficiaryAddress (post-sanitisation) | must not be blank |
| PaymentReference | must not be blank; sanitised value must not exceed 105 characters |

## Sanitisation (Pre-Validation)

IMT is the **reference model** for this pattern: `ImtValidator.Sanitize` runs in
`ConvertImtBatchHandler` *before* `ImtValidator.Validate` is ever called, so validation and mapping
only ever see already-cleaned values:

| Field | Sanitisation applied |
|---|---|
| DestinationBankAccountNo | spaces/hyphens/commas stripped |
| BeneficiaryAddress | disallowed characters (anything but letters/digits/space/hyphen/apostrophe) replaced with a space, repeated spaces collapsed, then truncated to 40 characters |
| IntermediaryBankName | same character sanitisation as above, then truncated to 30 characters |

This is a genuine sanitisation (content is stripped/shortened), not a transformation - contrast with
fields like Process Date (`yyMMdd` formatting) or the country-code derivation from a SWIFT BIC,
which reshape a value into a different format without discarding any of its meaning.

**Reject-vs-sanitise split:** `DestinationBankAccountName`, `DestinationBankName`, and the SWIFT
codes are rejected outright on invalid characters or overlong values - never silently altered.
`PaymentReference` is sanitised (disallowed characters replaced with a space) but *still* rejected if
the sanitised value is too long. Only `DestinationBankAccountNo`, `BeneficiaryAddress`, and
`IntermediaryBankName` are sanitised and truncated rather than rejected.

## Pattern Compliance (request → sanitisation → validation → map)

✅ **Fully compliant.** Sanitisation runs first (`ImtValidator.Sanitize`), validation runs against
the sanitised instruction, and mapping (`ImtRecordMapper`) writes the same sanitised values it was
validated with - there is no field that is altered *after* validation has already passed. This is
the pattern the other slices should be measured against.
