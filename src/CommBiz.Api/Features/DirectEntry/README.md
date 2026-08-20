# Direct Entry Slice

The Direct Entry vertical slice (ADR-002). Owns its own request model, validation, mapping,
and output-assembly logic for converting Direct Entry payment instructions end to end
(F-003–F-008, F-014): request/response contract, field validation, detail record mapping
(plus the F-014 self-balancing contra record), header/trailer assembly with self-balancing
totals, and final fixed-width file assembly. Dispatched to from the top-level Payment Type
Router (`Features/PaymentRouting`, F-015) — this slice no longer owns routing itself; F-004's
original DirectEntry-local routing check was removed once F-015 centralized it.

Future payment-type slices should follow this same convention: one folder per payment type
under `Features/`.

## Field Mapping

Where each CBA field comes from: `request.*` (dynamic, per-instruction), `appsetting.*`
(`DirectEntrySettings`, static config), or `constants.*` (hardcoded in the mapper). Fields
computed from the whole batch (sums, counts, min date) are still dynamic — noted as
`request.* (aggregate)`. Width is the fixed number of characters the field occupies in the
output record (each record below totals 120 characters).

### Header Record (`DirectEntryHeaderRecordMapper`)

| CBA Field | Width | Source | Transformation |
|---|---|---|---|
| Record Type | 1 | constants.RecordType | none |
| Blank | 17 | not mapped | literal spaces |
| Reel Sequence Number | 2 | constants.ReelSequenceNumber | none |
| Name Of User Financial Institution | 3 | constants.InstitutionCode | none |
| Blank | 7 | not mapped | literal spaces |
| Name of User Supplying File | 26 | constants.NameOfUserSupplyingFile | fixed-width pad/truncate to 26 |
| Number of User Supplying File | 6 | constants.UserIdentificationNumber | pad left to 6 with `0` |
| Description of Entries on File | 12 | appsetting.DescriptionOfEntriesOnFile | fixed-width pad/truncate to 12 |
| Date to be Processed | 6 | request.PaymentDate (aggregate: earliest in batch) | formatted `ddMMyy` |
| Blank | 40 | not mapped | literal spaces |

### Detail Record (`DirectEntryDetailRecordMapper`)

| CBA Field | Width | Source | Transformation |
|---|---|---|---|
| Record Type | 1 | constants.RecordType | none |
| BSB Number | 7 | request.DestinationBankBsb | inserted `-` (e.g. `015141` → `015-141`) |
| Account Number to be Credited/Debited | 9 | request.DestinationBankAccountNo | pad left to 9 |
| Indicator | 1 | constants.Indicator | none |
| Transaction Code | 2 | constants.CreditTransactionCode | none |
| Amount | 10 | request.Amount | converted to cents, pad left to 10 with `0` |
| Title of Account to be Credited/Debited | 32 | request.DestinationBankAccountName | fixed-width pad/truncate to 32 |
| Lodgement Reference | 18 | appsetting.LodgementReferenceDetails | fixed-width pad/truncate to 18 |
| Trace BSB Number | 7 | appsetting.TraceAccountBsb | none |
| Trace Account Number | 9 | appsetting.TraceAccountAccNo | pad left to 9 |
| Name of Remitter | 16 | appsetting.NameOfRemitter | fixed-width pad/truncate to 16 |
| Amount of withholding tax | 8 | appsetting.AmountOfWithholdingTax | none |

### Self-Balancing / Contra Detail Record (`DirectEntrySelfBalancingRecordMapper`)

Posts the batch total against Shaw's own settlement account — every field is static except Amount.
Same layout/widths as the Detail Record above.

| CBA Field | Width | Source | Transformation |
|---|---|---|---|
| Record Type | 1 | constants.RecordType | none |
| BSB Number | 7 | appsetting.TraceAccountBsb | none |
| Account Number to be Credited/Debited | 9 | appsetting.SelfBalancingAccountNo | pad left to 9 |
| Indicator | 1 | constants.Indicator | none |
| Transaction Code | 2 | constants.DebitTransactionCode | none |
| Amount | 10 | request.Amount (aggregate: sum across batch) | pad left to 10 with `0` |
| Title of Account to be Credited/Debited | 32 | constants.Title | fixed-width pad/truncate to 32 |
| Lodgement Reference | 18 | appsetting.SelfBalancingLodgementReferenceDetails | fixed-width pad/truncate to 18 |
| Trace BSB Number | 7 | appsetting.TraceAccountBsb | none |
| Trace Account Number | 9 | appsetting.SelfBalancingAccountNo | pad left to 9 |
| Name of Remitter | 16 | appsetting.SelfBalancingNameOfRemitter | fixed-width pad/truncate to 16 |
| Amount of withholding tax | 8 | appsetting.AmountOfWithholdingTax | none |

### Trailer Record (`DirectEntryTrailerRecordMapper`)

| CBA Field | Width | Source | Transformation |
|---|---|---|---|
| Record Type | 1 | constants.RecordType | none |
| BSB Number | 7 | constants.BsbNumber | none |
| Blank | 12 | not mapped | literal spaces |
| File (User) Net Total Amount | 10 | constants.NetTotalAmount | none (always zero, self-balancing) |
| File (User) Credit Total Amount | 10 | request.Amount (aggregate: sum across batch) | pad left to 10 with `0` |
| File (User) Debit Total Amount | 10 | request.Amount (aggregate: sum across batch) | pad left to 10 with `0` |
| Blank | 24 | not mapped | literal spaces |
| File (User) Count of Record Type 1 | 6 | request (aggregate: instruction count + 1 for contra) | pad left to 6 with `0` |
| Blank | 40 | not mapped | literal spaces |
