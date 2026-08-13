# Handoff: Shaw and Partners → CommBank Payment File Conversion Service

> Tranche: v1
> Last updated: 2026-08-14

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
- F-004 — Payment Type Router (whole-batch rejection on unsupported `paymentTypeCode`, case-insensitive `"DE"` match)
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

## Current State

- Service builds, tests, and runs cleanly (79 tests passing, 0 failed).
- Kestrel-hosted; Wolverine-wired.
- Full Direct Entry batch conversion pipeline works end to end: route → validate → map → assemble,
  returning the result inline as JSON per [ADR-008](adr/ADR-008-inline-json-response.md).
- `POST /convert` accepts a batch of as few as 1 valid payment instruction (previously required 2)
  and always returns a file whose trailer totals reconcile to zero net, thanks to the F-014
  self-balancing record.
- `GET /health` returns a basic liveness check.

## What's Next

**Phase 2 — Cross-Cutting Concerns & Hardening**
- F-009 — Shaw.Diagnostics logging + redaction
- F-010 — Document the internal-only deployment boundary
- F-011 — Batch size/latency targets — blocked on Architecture Open Question A4 / PM-002

**Phase 3 — Release Readiness**
- F-012 — Kestrel-only hosting config
- F-013 — E2E test coverage

## Important Context

1. **Open HTTP status-code product decision (PM-003)** — [docs/test-cases.md](test-cases.md)'s
   TC-004–TC-012 expect 4xx responses on rejection, but the API deliberately returns `200 OK` with a
   `{Success:false, Errors}` envelope for all rejection paths (router, validation,
   minimum-instruction-count). This needs a product-owner decision before Phase 3's E2E suite (F-013)
   or docs/test-cases.md are finalized.
2. **Open `DescriptionOfEntriesOnFile` truncation issue (PM-003)** — the configured value
   `"ONLINEPAYMENTS"` (14 chars) exceeds the Header spec's 12-char field width and is silently
   truncated to `"ONLINEPAYMEN"`. Needs a confirmed shorter value before production use.
3. **DirectEntry slice location** — core files live under
   `src/CommBiz.Api/Features/DirectEntry/` (Request/Response DTOs, Command+Handler, `PaymentTypeRouter`,
   `DirectEntryValidator`, `DirectEntryDetailRecordMapper`, `DirectEntryHeaderRecordMapper`,
   `DirectEntryTrailerRecordMapper`, `DirectEntrySelfBalancingRecordMapper`, `DirectEntryAmountTotals`)
   — all pure static-function mappers, no AutoMapper ([ADR-004](adr/ADR-004-manual-mapping-no-automapper.md)).
4. **Build/test/format commands** — the solution builds via `dotnet build CommBiz.sln`, tests via
   `dotnet test`, format via `dotnet format`.
