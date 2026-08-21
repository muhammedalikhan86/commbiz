# FX Slice

The FX vertical slice (ADR-002, F-022/F-023). Owns its own request model, validation, mapping, and
output-assembly logic for converting Commbiz Markets Bulk Settlement (IPFX) payment batches end to
end, per `docs/stash/CommBiz IPFX Bulk Settlement Upload - File Specification v4.0 2.md`:
request/response contract, field validation (`FxValidator`), 27-field CSV row mapping
(`FxRecordMapper`), and file assembly (`ConvertFxBatchCommand`/`ConvertFxBatchHandler`). Dispatched
to from the top-level Payment Type Router (`Features/PaymentRouting`) on the API's `"FOREX"`
payment type code (F-022).

Like IMT and Priority Payments, the FX file has **no header or trailer record** - it is simply one
27-field CSV row per instruction, CRLF-separated, with **no trailing CRLF after the last row**.

I SELL Instruction (field 7) and I BUY Instruction (field 12) always write the configured `MAN`/
`DOC` constants from `FxSettings`, never the request - Shaw and Partners never routes an FX
settlement to a real account (the spec's Samples 1/3/4/5 "Address Book Beneficiary" pattern). The
Amount is always placed on the Sell side (field 6) - the I BUY Amount (field 4) is always left
blank, per the spec's Sample 2. The beneficiary/intermediary bank fields (8/9/10/11/13/14/20),
however, are still mapped through from the request whenever present - they use the same shared
payload fields (`IntermediaryBankSwiftCode`, `DestinationBankSwiftCode`, `DestinationBankAccountName`,
`BeneficiaryAddress`) that IMT already maps for its own beneficiary/intermediary bank fields, so
they aren't left permanently blank just because Shaw's current flows happen not to populate them.
Fields 15/16 (Beneficiary Address lines 2/3) are always blank regardless - a hard spec rule, not a
data gap: "the payment will be rejected if a value is specified." Fields 17-19 (Beneficiary City/
State/Postcode) stay blank too, since there's no discrete field for them anywhere in the payload
(same gap IMT's README documents). The IDR/CNH/KRW conditional fields (23-27: Purpose of Payment,
CNAPS Code, Beneficiary Company Name, Beneficiary Contact Number, SSN) are deferred per
`docs/architecture.md` Open Question A6 and always left blank - no source data for them exists in
the payload at all, unlike 8-20.

`FxValidator` rejects the whole batch on any field failure - no partial conversion - and additionally
enforces file-level rules: 1-200 instructions per file, and at most 15 distinct currency pairs
(`BuyCurrency`/`SellCurrency` combinations) per file. The new beneficiary/intermediary bank fields
are pass-through only (truncated to their max field length in `FxRecordMapper`, per Fields 8/10/13/14's
AN limits) - no character-set or reject-on-invalid validation is applied to them yet.

`appsettings.json`'s `Fx` section (`SellInstruction`, `BuyInstruction`, `BuyPaymentDetails`,
`SellPaymentDetails`) uses real, confirmed values (F-023), not placeholders.

## Field Mapping

Where each CBA field comes from: `request.*` (dynamic, per-instruction) or `appsetting.*`
(`FxSettings`, static config). Position is the field's 1-based index in the comma-separated CSV row
(27 fields total - no header/trailer record).

### Payment Row (`FxRecordMapper`)

| CBA Field | Position | Source | Transformation |
|---|---|---|---|
| Transaction Type | 1 | constants.TransactionType | none (always `FX`) |
| Transaction Description | 2 | request.AccountNo | none |
| I BUY Currency | 3 | request.BuyCurrency | none |
| I BUY Amount | 4 | not applicable - Amount is always placed on the Sell side | empty |
| I SELL Currency | 5 | request.SellCurrency | none |
| I SELL Amount | 6 | request.Amount | none |
| I SELL Instruction | 7 | appsetting.SellInstruction | none (always `MAN` - Shaw never settles via a real account) |
| Intermediary Bank - Bank Code | 8 | request.IntermediaryBankSwiftCode | truncated to 11 characters; blank if not provided |
| Intermediary Institution - Country | 9 | request.IntermediaryBankSwiftCode (aggregate) | derived: characters 5-6 of the SWIFT BIC; blank if not provided |
| Beneficiary Bank - Bank Code | 10 | request.DestinationBankSwiftCode | truncated to 11 characters; blank if not provided |
| Beneficiary Bank - Country | 11 | request.DestinationBankSwiftCode (aggregate) | derived: characters 5-6 of the SWIFT BIC; blank if not provided |
| I BUY Instruction | 12 | appsetting.BuyInstruction | none (always `DOC` - Shaw never settles via a real account) |
| Beneficiary - Account Name | 13 | request.DestinationBankAccountName | truncated to 62 characters; blank if not provided |
| Beneficiary - Address line 1 | 14 | request.BeneficiaryAddress | truncated to 40 characters; blank if not provided |
| Beneficiary - Address line 2 | 15 | not applicable | empty - always blank per spec rule, payment is rejected if populated |
| Beneficiary - Address line 3 | 16 | not applicable | empty - always blank per spec rule, payment is rejected if populated |
| Beneficiary - City/Suburb | 17 | no discrete field in the payload | empty |
| Beneficiary - State | 18 | no discrete field in the payload | empty |
| Beneficiary - Postcode | 19 | no discrete field in the payload | empty |
| Beneficiary - Country | 20 | request.DestinationBankSwiftCode (aggregate) | derived: same as field 11 |
| I BUY Payment details | 21 | appsetting.BuyPaymentDetails | none |
| I SELL Payment details | 22 | appsetting.SellPaymentDetails | none |
| Purpose of Payment | 23 | IDR/CNH-specific, deferred (A6) - no source field exists in the payload | empty |
| CNAPS Code | 24 | CNH-specific, deferred (A6) | empty |
| Beneficiary Company Name | 25 | KRW-specific, deferred (A6) | empty |
| Beneficiary Contact Number | 26 | KRW-specific, deferred (A6) | empty |
| Social Security Number (SSN) | 27 | KRW-specific, deferred (A6) | empty |

## Exception List (Non-Negotiables)

Conditions checked by `FxValidator` that cause the whole batch to be thrown back (rejected) rather
than converted. Any one of these, on any single instruction, rejects the entire file - no partial
conversion.

| Field | Non-negotiable rule |
|---|---|
| Batch size | 1-200 payment instructions per file |
| Currency pairs | at most 15 distinct `BuyCurrency`/`SellCurrency` combinations per file |
| BuyCurrency / SellCurrency | exactly 3 uppercase alphabetic characters |
| Amount | positive, at most 11 integer + 2 decimal digits (≤ 99,999,999,999.99) |
| AccountNo | 1-12 alphanumeric characters |

**Not currently in the exception list:** `IntermediaryBankSwiftCode`, `DestinationBankSwiftCode`,
`DestinationBankAccountName`, and `BeneficiaryAddress` have no validation at all - there is no
non-negotiable rule for them yet, so no value (including invalid or overlong ones) is ever rejected.

## Sanitisation (Pre-Validation)

Fx has **no pre-validation sanitisation step**. `IntermediaryBankSwiftCode`/`DestinationBankSwiftCode`
(truncated to 11 characters), `DestinationBankAccountName` (62 characters), and `BeneficiaryAddress`
(40 characters) are all truncated in `FxRecordMapper`, in the mapping layer only. This is already
called out earlier in this README as an intentional, deferred gap: "pass-through only... no
character-set or reject-on-invalid validation is applied to them yet."

## Pattern Compliance (request → sanitisation → validation → map)

⚠️ **Violation (acknowledged/deferred).** Same shape as Direct Entry's gap, but broader: four
fields are truncated in the mapper with zero validation - not even a not-blank check - so the
pattern is effectively request → map(with untracked truncation), skipping both sanitisation and
validation for these fields. Unlike Direct Entry's gap, this one is already documented as a known,
accepted limitation in this slice's own README (new beneficiary/intermediary bank fields are
"pass-through only"), so it's listed here as a pre-existing, team-accepted exception rather than a
new finding - but it should be tightened (add character-set/length validation, or an explicit
pre-validation sanitise step) before these fields carry any Shaw-and-Partners-critical data.
