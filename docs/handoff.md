# Handoff: Shaw and Partners → CommBank Payment File Conversion Service

> Tranche: v1
> Last updated: 2026-08-17

## What Was Done

**Phase 1 — Direct Entry Conversion Core is fully implemented and passing all quality gates,
including the F-014 self-balancing amendment.**

- F-001 — Scaffolded the Minimal API host (.NET 10, Kestrel, vertical-slice structure)
- F-002 — Wired up Wolverine (in-process CQRS/command dispatch)
- F-003 — Direct Entry request/response contract, reshaped to Shaw and Partners' native upstream
  payload shape: `POST /convert` accepts a plain JSON array of `PaymentInstructionRequest`
  (`paymentTypeCode`, `accountNo`, `paymentSourceTypeCode`, `sourceBankAccountName`,
  `sourceBankAccountNo`, `sourceBankBsb`, `paymentDate`, `sourceCurrency`, `sourceAmount`, `amount`,
  `createBy`); organisation-level constants (title, lodgement reference, trace BSB/account, remitter
  name, indicator, transaction code, withholding tax) are sourced from `appsettings.json`'s
  `DirectEntry` section via `DirectEntrySettings`, not the request
- F-004 — Payment Type Router (whole-batch rejection on unsupported `paymentTypeCode`, case-insensitive `"DE"` match). Originally a DirectEntry-local check; removed once F-015 centralized this rule into the top-level cross-slice router (`Features/PaymentRouting`), which alone now enforces it
- F-005 — Direct Entry field validation (BSB/account/indicator/transaction code/amount/mandatory field rules)
- F-006 — Detail record mapping (120-char fixed-width Detail Record, manual mapping per [ADR-004](adr/ADR-004-manual-mapping-no-automapper.md))
- F-007 — Header/Trailer record assembly with self-balancing totals (credit/debit/net/count reconciliation)
- F-008 — Final file assembly: full Header+Details+Trailer response proven end-to-end via HTTP test
- F-014 — Self-balancing (contra) detail record + minimum batch size reduction: a self-balancing
  detail record is now generated for every conversion, posting the batch's total amount (in cents)
  against the configured settlement account (`TraceAccountBsb`/`TraceAccountAccNo`) in the transaction
  direction opposite the configured `TransactionCode`, positioned immediately before the trailer
  record. Implemented in `DirectEntrySelfBalancingRecordMapper.cs` (new) and
  `DirectEntryAmountTotals.cs` (new shared cents-total helper, per PM-005 — one computation shared by
  both the self-balancing mapper and the trailer mapper so they can never disagree by a cent).
  `ConvertDirectEntryBatchCommand.cs` now wires the self-balancing record in between the real detail
  records and the trailer. `DirectEntryTrailerRecordMapper.cs` was updated so credit/debit totals
  always include the self-balancing record, making net total always zero. `DirectEntryValidator.cs`'s
  minimum instruction count was reduced from 2 to 1, since the self-balancing record itself now
  satisfies the Direct Entry spec's minimum-2-detail-record rule.

Integration Agent then: fixed a `WolverineFx.RuntimeCompilation` missing-dependency issue; ran the
full test suite (79 passed, 0 failed), format, build, and security audit — all green.

**Phase 2 — Additional Payment Types is underway: F-015, F-016, and F-017 are Done; F-018
(Priority Payments) is next, still Planned and blocked on PM-006.**

- F-015 — Extended the Payment Type Router into a real top-level cross-slice dispatcher
  (`Features/PaymentRouting`): it peeks `paymentTypeCode` on the raw JSON batch, rejects
  empty/mixed-type/unsupported batches in full, and dispatches to the correct slice's own
  Wolverine command. Files: `PaymentTypeRouter.cs`, `PaymentRoutingResponse.cs`, `README.md`;
  `Program.cs` updated to route through it. Delivered scope: DE and BPAY wired at the time; IMT
  and Priority Payments were the pending work (IMT is now also done, via F-017).
- F-016 — Full BPAY Batch Payments conversion slice (validator, header/detail CSV mappers,
  handler), replacing the F-015 stub. No trailer/self-balancing record — not part of the BPay
  spec. Files: `src/CommBiz.Api/Features/BPay/*.cs`, `README.md`. Known open item:
  `BPaySettings.FundingAccount`/`FileNumber` are still placeholder values (PM-007).
- F-017 — Full IMT (International Money Transfers) conversion slice: 27-field MT101-family CSV,
  SWIFT-derived country codes, reject-vs-sanitize field handling, no-trailing-CRLF assembly. The
  router was extended to recognise the API's `TT` code (mapped to the file's `IMT` Transaction
  Type constant). Files: `src/CommBiz.Api/Features/Imt/*.cs`, `README.md`. `ImtSettings` config
  values are confirmed real values, not placeholders.

## Current State

- Service builds, tests, and runs cleanly (206 tests passing, 0 failed).
- Kestrel-hosted; Wolverine-wired.
- Three payment types convert end to end through the shared router: route → validate → map →
  assemble, returning the result inline as JSON per [ADR-008](adr/ADR-008-inline-json-response.md).
  - Direct Entry — `POST /convert` accepts a batch of as few as 1 valid payment instruction
    (previously required 2) and always returns a file whose trailer totals reconcile to zero net,
    thanks to the F-014 self-balancing record.
  - BPAY Batch Payments — converts to the BPay CSV layout (Header + one Payment Details record per
    instruction, no trailer/self-balancing record).
  - International Money Transfers (IMT) — converts to the 27-field MT101-family CSV layout.
- Priority Payments (F-018, also known as RTGS) is not yet implemented — still Planned, blocked on
  PM-006 (confirmed request-object shape not yet provided).
- `GET /health` returns a basic liveness check.

## What's Next

**Phase 2 — Additional Payment Types (current phase)**
- F-018 — Priority Payments (also known as RTGS), sharing the MT101 format with IMT — blocked on
  PM-006 until the upstream request-object shape is confirmed

**Phase 3 — Cross-Cutting Concerns & Hardening**
- F-009 — Shaw.Diagnostics logging + redaction
- F-010 — Document the internal-only deployment boundary
- F-011 — Batch size/latency targets — blocked on Architecture Open Question A4 / PM-002

**Phase 4 — Release Readiness**
- F-012 — Kestrel-only hosting config
- F-013 — E2E test coverage

## Important Context

1. **Open HTTP status-code product decision (PM-003)** — [docs/test-cases.md](test-cases.md)'s
   TC-004–TC-012 expect 4xx responses on rejection, but the API deliberately returns `200 OK` with a
   `{Success:false, Errors}` envelope for all rejection paths (router, validation,
   minimum-instruction-count). This needs a product-owner decision before Phase 4's E2E suite (F-013)
   or docs/test-cases.md are finalized.
2. **Open `DescriptionOfEntriesOnFile` truncation issue (PM-003)** — the configured value
   `"ONLINEPAYMENTS"` (14 chars) exceeds the Header spec's 12-char field width and is silently
   truncated to `"ONLINEPAYMEN"`. Needs a confirmed shorter value before production use.
3. **Slice locations** — the router now lives in its own top-level slice,
   `src/CommBiz.Api/Features/PaymentRouting/` (`PaymentTypeRouter`, `PaymentRoutingResponse`), and
   dispatches to each payment type's own slice:
   - `src/CommBiz.Api/Features/DirectEntry/` (Request/Response DTOs, Command+Handler,
     `DirectEntryValidator`, `DirectEntryDetailRecordMapper`, `DirectEntryHeaderRecordMapper`,
     `DirectEntryTrailerRecordMapper`, `DirectEntrySelfBalancingRecordMapper`, `DirectEntryAmountTotals`)
   - `src/CommBiz.Api/Features/BPay/` (validator, header/detail CSV mappers, Command+Handler)
   - `src/CommBiz.Api/Features/Imt/` (validator, CSV mapper, Command+Handler)
   All mapping is done via pure static functions, no AutoMapper ([ADR-004](adr/ADR-004-manual-mapping-no-automapper.md)).
4. **Build/test/format commands** — the solution builds via `dotnet build CommBiz.sln`, tests via
   `dotnet test`, format via `dotnet format`.
5. **RTGS is Priority Payments, not a separate payment type (PM-008)** — the user clarified RTGS is
   Shaw and Partners' own name for Priority Payments (F-018); it does not require separate routing
   or conversion work. The formerly tracked Non-CBA Payment Requests, Payables Direct, and NZ BECS
   (MT9) work (F-019/F-020) is cancelled, not deferred — see PM-008 and PMBook v11.
6. **F-018 (Priority Payments) is the next feature up in Phase 2**, still Planned — blocked on
   PM-006 until the upstream request-object shape (analogous to BPAY's and IMT's confirmed shapes)
   is provided. The MT101 spec's §1.5 Priority Payment Field Definition is already available, so
   once the request shape lands this should be a small additive change to the existing router and
   slice pattern, not a new design effort.
