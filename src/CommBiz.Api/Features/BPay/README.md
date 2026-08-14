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

**Assumption needing confirmation:** `appsettings.json`'s `BPay` section (`FundingAccount`,
`FileNumber`) uses placeholder values only, no real funding account has been confirmed yet
(same open-config treatment as Direct Entry's PM-003 values).
