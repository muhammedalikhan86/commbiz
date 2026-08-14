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
`TraceAccountBsb`/`TraceAccountAccNo` and BPay's `FundingAccount` pattern.

**Reject-vs-sanitize split (per the confirmed field mapping):** legal/identity data (Beneficiary
Account Number, Beneficiary Account Name, bank names, SWIFT codes) is rejected outright on invalid
characters or overlong values - never silently altered. Free-text postal data (Beneficiary Address)
and reference/remittance text (Beneficiary Payment Details) are sanitized (disallowed characters,
including commas, replaced with a space) before the length limit is enforced.

`appsettings.json`'s `Imt` section (`DebitAccountBsb`, `DebitAccountNumber`, `DebitAccountName`)
uses real, confirmed values (F-017), not placeholders.
