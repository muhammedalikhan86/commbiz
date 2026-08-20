# BPAY Slice

The BPAY vertical slice (ADR-002, F-016). Owns its own request model, validation, mapping,
and output-assembly logic for converting BPAY Batch Payments end to end, per
`docs/stash/BPay Payments - CommBiz File Specification.md`: request/response contract, field
validation (`BPayValidator`), header/detail record mapping (`BPayHeaderRecordMapper` /
`BPayDetailRecordMapper`), and CSV file assembly (Header + one Payment Details record per
instruction, CRLF-terminated). Dispatched to from the top-level Payment Type Router
(`Features/PaymentRouting`), added in F-015.

Unlike Direct Entry, BPay's output is CSV (comma-delimited, not fixed-width) and has no
trailer or self-balancing record — the file is simply Header + Details.

`appsettings.json`'s `BPay` section (`FundingAccount`, `FileNumber`) uses real, confirmed values
(same settlement account IMT/Priority Payments/FX already use), not placeholders.

## Field Mapping

Where each CBA field comes from: `request.*` (dynamic, per-instruction), `appsetting.*`
(`BPaySettings`, static config), or `constants.*` (hardcoded in the mapper). Fields computed from
the whole batch (sums, counts, min date) are still dynamic — noted as `request.* (aggregate)`.
Position is the field's 1-based index in the comma-separated record (BPay is CSV, not fixed-width,
so there is no width column here).

### Header Record (`BPayHeaderRecordMapper`)

| CBA Field | Position | Source | Transformation |
|---|---|---|---|
| Record Type | 1 | constants.RecordType | none |
| File Creation Date | 2 | request (aggregate: `DateTime.UtcNow`) | formatted `yyyyMMdd` |
| File Creation Time | 3 | request (aggregate: `DateTime.UtcNow`) | formatted `HHmmss` |
| File Number | 4 | appsetting.FileNumber | none |
| Payment Account | 5 | appsetting.FundingAccount | none |
| Payment Date | 6 | request.PaymentDate (aggregate: earliest in batch) | formatted `yyyyMMdd` |
| Number of Payment Records | 7 | request (aggregate: instruction count) | none |
| Total Amount of Payments | 8 | request.Amount (aggregate: sum across batch) | converted to cents |

### Payment Details Record (`BPayDetailRecordMapper`)

Unlike Direct Entry, there is no trailer or self-balancing record — every instruction maps to
exactly one Payment Details record, and every position the spec doesn't assign to this record type
is left blank.

| CBA Field | Position | Source | Transformation |
|---|---|---|---|
| Record Type | 1 | constants.RecordType | none |
| File Creation Date | 2 | not mapped | empty |
| File Creation Time | 3 | not mapped | empty |
| File Number | 4 | not mapped | empty |
| Payment Account | 5 | not mapped | empty |
| Payment Date | 6 | not mapped | empty |
| Number of Payment Records | 7 | not mapped | empty |
| Currency Code of Payment | 8 | not mapped | empty |
| Biller Code | 9 | request.BPayBillerCode | none |
| Service Code | 10 | not mapped | empty |
| Customer Reference Number | 11 | request.BPayReference | none |
| Payment Method | 12 | not mapped | empty |
| Entry Method | 13 | not mapped | empty |
| Amount | 14 | request.Amount | converted to cents |
| Transaction Reference Number | 15 | not mapped | empty |
| Original Reference Number | 16 | not mapped | empty |
| BPAY Settlement Date | 17 | not mapped | empty |
| Date Payment Accepted | 18 | not mapped | empty |
| Time Payment Accepted | 19 | not mapped | empty |
| Payer Name | 20 | not mapped | empty |
| Additional Reference Code | 21 | not mapped | empty |
| Error Correction Reason | 22 | not mapped | empty |
| Discount Method | 23 | not mapped | empty |
| Discount Reference | 24 | not mapped | empty |
| Discretionary Data | 25 | not mapped | empty |
