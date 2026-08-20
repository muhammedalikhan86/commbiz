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

Only the IPFX spec's non-CBA "Instruction" pattern is supported (Sample 2: I SELL Instruction =
`MAN`, I BUY Instruction = `DOC`, both sourced from `FxSettings`, never the request). The Amount
is always placed on the Sell side (field 6) - the I BUY Amount (field 4) is always left blank, per
the spec's sample. The IDR/CNH/KRW conditional fields (23-27: Purpose of Payment, CNAPS Code,
Beneficiary Company Name, Beneficiary Contact Number, SSN) are deferred per `docs/architecture.md`
Open Question A6 and always left blank.

`FxValidator` rejects the whole batch on any field failure - no partial conversion - and additionally
enforces file-level rules: 1-200 instructions per file, and at most 15 distinct currency pairs
(`BuyCurrency`/`SellCurrency` combinations) per file.

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
| I SELL Instruction | 7 | appsetting.SellInstruction | none |
| Intermediary Bank - Bank Code | 8 | not applicable | empty |
| Intermediary Institution - Country | 9 | not applicable | empty |
| Beneficiary Bank - Bank Code | 10 | not applicable | empty |
| Beneficiary Bank - Country | 11 | not applicable | empty |
| I BUY Instruction | 12 | appsetting.BuyInstruction | none |
| Beneficiary - Account Name | 13 | not applicable | empty |
| Beneficiary - Address line 1 | 14 | not applicable | empty |
| Beneficiary - Address line 2 | 15 | not applicable | empty |
| Beneficiary - Address line 3 | 16 | not applicable | empty |
| Beneficiary - City/Suburb | 17 | not applicable | empty |
| Beneficiary - State | 18 | not applicable | empty |
| Beneficiary - Postcode | 19 | not applicable | empty |
| Beneficiary - Country | 20 | not applicable | empty |
| I BUY Payment details | 21 | appsetting.BuyPaymentDetails | none |
| I SELL Payment details | 22 | appsetting.SellPaymentDetails | none |
| Purpose of Payment | 23 | IDR/CNH-specific, deferred (A6) | empty |
| CNAPS Code | 24 | CNH-specific, deferred (A6) | empty |
| Beneficiary Company Name | 25 | KRW-specific, deferred (A6) | empty |
| Beneficiary Contact Number | 26 | KRW-specific, deferred (A6) | empty |
| Social Security Number (SSN) | 27 | KRW-specific, deferred (A6) | empty |
