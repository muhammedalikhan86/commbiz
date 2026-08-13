# Handoff: Shaw and Partners → CommBank Payment File Conversion Service

> Tranche: v1
> Last updated: 2026-08-13

## What Was Done

**Phase 1 — Direct Entry Conversion Core is fully implemented and passing all quality gates.**

- F-001 — Scaffolded the Minimal API host (.NET 10, Kestrel, vertical-slice structure)
- F-002 — Wired up Wolverine (in-process CQRS/command dispatch); the placeholder `/diagnostics/ping` endpoint proves the pipeline
- F-003 — Direct Entry request/response contract (DTOs, `POST /direct-entry/convert` endpoint, placeholder echo conversion)
- F-004 — Payment Type Router (whole-batch rejection on unsupported `PaymentType`, case-insensitive `"DirectEntry"` match)
- F-005 — Direct Entry field validation (BSB/account/indicator/transaction code/amount/mandatory field rules, header-level rules with `Index=-1` sentinel)
- F-006 — Detail record mapping (120-char fixed-width Detail Record, manual mapping per [ADR-004](adr/ADR-004-manual-mapping-no-automapper.md))
- F-007 — Header/Trailer record assembly with self-balancing totals (credit/debit/net/count reconciliation)
- F-008 — Final file assembly: minimum-2-instruction structural rule enforced; full Header+Details+Trailer response proven end-to-end via HTTP test

Integration Agent then: fixed a `WolverineFx.RuntimeCompilation` missing-dependency issue (all endpoint
tests were failing to boot the host without it); fixed a stale `Features/DirectEntry/README.md`; ran the
full test suite (106 passed), format, build, and security audit — all green. Integration flagged one
unresolved product decision (see Important Context below) but did not block on it.

## Current State

- Service builds, tests, and runs cleanly.
- Kestrel-hosted; Wolverine-wired.
- Full Direct Entry batch conversion pipeline works end to end: route → validate → map → assemble,
  returning the result inline as JSON per [ADR-008](adr/ADR-008-inline-json-response.md).

## What's Next

**Phase 2 — Cross-Cutting Concerns & Hardening**
- F-009 — Shaw.Diagnostics logging + redaction
- F-010 — Document the internal-only deployment boundary
- F-011 — Batch size/latency targets — blocked on Architecture Open Question A4 / PM-002

**Phase 3 — Release Readiness**
- F-012 — Kestrel-only hosting config
- F-013 — E2E test coverage

## Important Context

1. **Open HTTP status-code product decision** — [docs/test-cases.md](test-cases.md)'s TC-004–TC-012
   expect 4xx responses on rejection, but the API deliberately returns `200 OK` with a
   `{Success:false, Errors}` envelope for all rejection paths (router, validation,
   minimum-instruction-count). This needs a product-owner decision before Phase 3's E2E suite (F-013)
   or docs/test-cases.md are finalized.
2. **DirectEntry slice location** — core files live under
   `src/CommBiz.Api/Features/DirectEntry/` (Request/Response DTOs, Command+Handler, `PaymentTypeRouter`,
   `DirectEntryValidator`, `DirectEntryDetailRecordMapper`, `DirectEntryHeaderRecordMapper`,
   `DirectEntryTrailerRecordMapper`) — all pure static-function mappers, no AutoMapper
   ([ADR-004](adr/ADR-004-manual-mapping-no-automapper.md)).
3. **Build/test/format commands** — the solution builds via `dotnet build CommBiz.sln`, tests via
   `dotnet test`, format via `dotnet format`.
