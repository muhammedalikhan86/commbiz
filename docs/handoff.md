# Handoff: Shaw and Partners → CommBank Payment File Conversion Service

> Tranche: v1
> Last updated: 2026-08-18

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

**Phase 2 — Additional Payment Types is now fully Done: F-015, F-016, F-017, F-018, and F-021 are
all Done.**

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
- F-021 — Introduced a shared Field Mapping Model (`FieldMapping`/`LineMapping`, new file
  `src/CommBiz.Api/Features/Shared/FieldMapping.cs`) and retrofitted Direct Entry, BPay, and IMT's
  conversion responses with a new `Mappings` field: an ordered list, parallel to `ConvertedText`,
  giving a per-line, per-field breakdown of request-side origin vs. CBA spec field/value — so the
  testing team can validate conversions without parsing raw fixed-width/CSV text. This is a second
  sanctioned [ADR-002](adr/ADR-002-vertical-slice-architecture.md) exception, alongside the Payment
  Type Router (see [ADR-009](adr/ADR-009-shared-field-mapping-response-model.md)). Took 1 review
  retry: round 1 caught a real bug where BPay's header timestamp was independently recomputed
  between the text-assembly and field-breakdown code paths, risking a one-second mismatch; fixed by
  resolving the timestamp once and passing it through explicitly.
- F-021 follow-up refactor (no new feature ID) — extracted the `AmountToCents`/`FixedWidth` helpers,
  previously duplicated byte-identically across 6 mapper files, into a new shared
  `src/CommBiz.Api/Features/Shared/MappingUtilities.cs`. Pure internal refactor, zero behavior
  change, PASS on first review.
- F-018 — Full Priority Payments (also known as RTGS) conversion slice. Routed on the API's
  `"RTGS"` payment type code (the CBA file format literal is always `"PP"`, the same pattern as
  IMT's `"TT"` → `"IMT"`). Shares IMT's 27-field CSV format (MT101 spec §1.5), but almost every
  SWIFT/currency/intermediary field is "Not applicable" for this domestic, BSB-based payment. Built
  directly against the shared Field Mapping Model from F-021, not retrofitted — `Mappings` present
  from day one. New `PriorityPaymentsSettings` config section with a confirmed real settlement
  account (same account IMT uses: `062-000`/`2112 0075`/`SHAW - AUD TRUST ACCOUNT`). Key
  business-rule differences from IMT: a 14-month process-date window (not IMT's 7 days), stricter
  beneficiary name/address character rules (no hyphen/apostrophe), and a plain 6-digit BSB (not
  hyphenated like Direct Entry). Files: `src/CommBiz.Api/Features/PriorityPayments/*.cs`,
  `README.md`, new `tests/smoke/PriorityPayments.http` (4 scenarios). Reviewer PASS on first
  attempt.

Integration Agent PASS (318 tests, 0 vulnerabilities) and Reviewer-Integration PASS (first
attempt) for F-018 — no fixes needed. With F-018 Done, all five Phase 2 features (F-015, F-016,
F-017, F-018, F-021) are now Done and the phase is closed.

**FX (Foreign Exchange) — F-022 and F-023 — added after the above, completing Phase 2's fifth and
final payment type.**

- F-023 — Full FX conversion slice: converts Shaw and Partners' `FOREX`-typed payment instructions
  into a CommBiz IPFX Bulk Settlement Upload CSV file (one data row per instruction, no
  header/trailer/self-balancing record). Field mapping: constant `FX` Transaction Type; `accountNo`
  → Transaction Description; Buy/Sell Currency and Amount mapped directly (Amount always on the Sell
  side, I BUY Amount left blank); I SELL/I BUY Instruction (`MAN`/`DOC`) and I BUY/I SELL Payment
  details (`Buy`/`Sell`) sourced from static `FxSettings` configuration, not the request; `Notes`
  carried but unused. Batch rules: 1-200 instructions, max 15 distinct currency pairs. Built directly
  against the shared Field Mapping Model (F-021) from day one, no retrofit needed — same pattern as
  Priority Payments. IDR/CNH/KRW-specific fields are explicitly deferred (Architecture Open Question
  A6). Files: `src/CommBiz.Api/Features/Fx/*.cs` (`FxPaymentInstructionRequest`, `FxSettings`,
  `FxValidator`, `FxRecordMapper`, `ConvertFxBatchCommand`/`ConvertFxBatchHandler`,
  `ConvertFxBatchResponse`); new `appsettings.json` `"Fx"` section, registered in `Program.cs`.
- F-022 — Extended the Payment Type Router
  (`src/CommBiz.Api/Features/PaymentRouting/PaymentTypeRouter.cs`) to recognise the `FOREX` code and
  dispatch to the FX slice, mirroring the existing DE/BPAY/TT/RTGS dispatch pattern. No changes to
  existing dispatch behaviour for the other four payment types.

Integration Agent PASS (374 tests, 0 vulnerabilities) and Reviewer-Integration PASS (first attempt)
for both F-022 and F-023 — no fixes needed. With F-023 Done, all seven Phase 2 features (F-015,
F-016, F-017, F-018, F-021, F-022, F-023) are now Done and **Phase 2 is fully complete: all five
payment types (Direct Entry, BPay, IMT, Priority Payments, FX) now convert end to end.**

New documentation gaps surfaced by this round, not yet resolved: [docs/test-cases.md](test-cases.md)
has zero FX scenarios and no `tests/smoke/Fx.http` file exists (tracked as new PMBook item PM-012);
the existing test-runbook/handoff/CHANGELOG/REVISION staleness item now also covers F-022/F-023
(amended PM-011).

## Current State

- Service builds, tests, and runs cleanly (374 tests passing, 0 failed, 0 vulnerabilities).
- Kestrel-hosted; Wolverine-wired.
- **Phase 1 (Direct Entry Conversion Core) and Phase 2 (Additional Payment Types) are both fully
  Done.** All five payment types now convert end to end through the shared router: route →
  validate → map → assemble, returning the result inline as JSON per
  [ADR-008](adr/ADR-008-inline-json-response.md).
  - Direct Entry — `POST /convert` accepts a batch of as few as 1 valid payment instruction and
    always returns a file whose trailer totals reconcile to zero net, thanks to the F-014
    self-balancing record.
  - BPAY Batch Payments — converts to the BPay CSV layout (Header + one Payment Details record per
    instruction, no trailer/self-balancing record).
  - International Money Transfers (IMT) — converts to the 27-field MT101-family CSV layout.
  - Priority Payments (RTGS) — converts to the same 27-field MT101-family CSV layout as IMT, with
    almost all SWIFT/currency/intermediary fields "Not applicable" for this domestic BSB-based
    payment; 14-month process-date window; stricter beneficiary character rules.
  - FX (Foreign Exchange) — routed on the `FOREX` code, converts to the CommBiz IPFX Bulk
    Settlement Upload CSV layout (one data row per instruction, no header/trailer/self-balancing
    record); IDR/CNH/KRW-specific fields are explicitly out of scope for now (Architecture Open
    Question A6).
- All five payment types' responses include a `Mappings` field (F-021), giving a per-line,
  per-field breakdown of request-origin vs. CBA-spec-output values, for testing-team verification
  without needing to parse the raw converted text. Priority Payments and FX were both built against
  this model from day one, with no retrofit needed.
- `GET /health` returns a basic liveness check.
- Phase 3 (Cross-Cutting Concerns & Hardening) and Phase 4 (Release Readiness) are both Planned,
  not yet started.
- Known gaps, tracked in the PMBook as PM-011 (amended) and the newly-added PM-012, not yet
  resolved: [docs/test-cases.md](test-cases.md) has zero scenarios for FX; no `tests/smoke/Fx.http`
  file exists (both PM-012); [docs/testing/phase-2-additional-payment-types-i.md](testing/phase-2-additional-payment-types-i.md),
  [ADR-009](adr/ADR-009-shared-field-mapping-response-model.md), this handoff doc, `CHANGELOG.md`,
  and `REVISION.md` are one feature-pair stale for F-022/F-023 (amended PM-011, previously scoped to
  F-018 only). These docs should be brought current — and PM-011/PM-012 resolved — before Phase 4's
  E2E test work (F-013) begins.

## What's Next

**Documentation gaps to close first**
- PM-012 (new) — add FX scenarios to [docs/test-cases.md](test-cases.md) and create
  `tests/smoke/Fx.http`
- PM-011 (amended) — bring [docs/testing/phase-2-additional-payment-types-i.md](testing/phase-2-additional-payment-types-i.md),
  [ADR-009](adr/ADR-009-shared-field-mapping-response-model.md), this handoff doc, `CHANGELOG.md`,
  and `REVISION.md` current for F-022/F-023 (previously amended for F-018 only)

**Phase 3 — Cross-Cutting Concerns & Hardening (current phase)**
- F-009 — Shaw.Diagnostics logging + redaction
- F-010 — Document the internal-only deployment boundary
- F-011 — Batch size/latency targets — blocked on Architecture Open Question A4 / PM-002

**Phase 4 — Release Readiness**
- F-012 — Kestrel-only hosting config
- F-013 — E2E test coverage — should follow closure of PM-011/PM-012's documentation gaps so the
  E2E suite has accurate FX and Priority Payments test-case coverage to build from

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
   - `src/CommBiz.Api/Features/PriorityPayments/` (validator, CSV mapper, Command+Handler)
   - `src/CommBiz.Api/Features/Fx/` (`FxPaymentInstructionRequest`, `FxSettings`, `FxValidator`,
     `FxRecordMapper`, `ConvertFxBatchCommand`/`ConvertFxBatchHandler`, `ConvertFxBatchResponse`)
   All mapping is done via pure static functions, no AutoMapper ([ADR-004](adr/ADR-004-manual-mapping-no-automapper.md)).
4. **Build/test/format commands** — the solution builds via `dotnet build CommBiz.sln`, tests via
   `dotnet test`, format via `dotnet format`.
5. **RTGS is Priority Payments, not a separate payment type (PM-008)** — the user clarified RTGS is
   Shaw and Partners' own name for Priority Payments (F-018); it does not require separate routing
   or conversion work. The formerly tracked Non-CBA Payment Requests, Payables Direct, and NZ BECS
   (MT9) work (F-019/F-020) is cancelled, not deferred — see PM-008 and PMBook v11.
6. **F-018 (Priority Payments) is Done** — built against the confirmed upstream request-object
   shape (PM-006 resolved), sharing the MT101 spec's §1.5 Priority Payment Field Definition with
   IMT. It is a domestic, BSB-based payment: almost all SWIFT/currency/intermediary fields are "Not
   applicable", the process-date window is 14 months (vs. IMT's 7 days), and beneficiary
   name/address fields disallow hyphens/apostrophes.
7. **The shared Field Mapping Model (F-021) is now used consistently by all five implemented
   payment types (DE, BPay, IMT, Priority Payments, FX)** — it is an established second ADR-002
   exception, alongside the Payment Type Router — see
   [ADR-009](adr/ADR-009-shared-field-mapping-response-model.md). It lives at
   `src/CommBiz.Api/Features/Shared/FieldMapping.cs` (`FieldMapping`/`LineMapping`), with the
   `AmountToCents`/`FixedWidth` helpers factored out to
   `src/CommBiz.Api/Features/Shared/MappingUtilities.cs`. Priority Payments (F-018) and FX (F-023)
   were both built directly against this model from day one, with no retrofit step required.
8. **PM-011/PM-012 track remaining documentation gaps, now including the FX round** —
   [docs/test-cases.md](test-cases.md) has zero FX scenarios and no `tests/smoke/Fx.http` file
   exists (PM-012, new); [docs/testing/phase-2-additional-payment-types-i.md](testing/phase-2-additional-payment-types-i.md),
   [ADR-009](adr/ADR-009-shared-field-mapping-response-model.md), this handoff doc, `CHANGELOG.md`,
   and `REVISION.md` are stale for F-022/F-023 (PM-011, amended — previously scoped to F-018 only).
   Close both before starting Phase 4's E2E test work (F-013), so the E2E suite has accurate source
   material to build from.
9. **FX's IDR/CNH/KRW-specific fields are explicitly out of scope for now (Architecture Open
   Question A6)** — `FxValidator`/`FxRecordMapper` handle the general Buy/Sell currency-pair case
   only; no currency-specific field handling exists yet for those three currencies. Revisit once A6
   is resolved.
