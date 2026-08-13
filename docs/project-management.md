# Project Management: Shaw and Partners → CommBank Payment File Conversion Service

> Status: DRAFT
> Tranche: v1
> Version: v1
> Last updated: 2026-08-13
> PRD: docs/prd.md (built against v6)
> Architecture: docs/architecture.md (built against v4)

## Phases & Milestones

| Phase | Goal | Status |
|-------|------|--------|
| Phase 1 | Direct Entry Conversion Core — accept a batch, route/validate/convert Direct Entry instructions, return the assembled file inline as JSON | Planned |
| Phase 2 | Cross-Cutting Concerns & Hardening — logging, security posture, performance targets | Planned |
| Phase 3 | Release Readiness — hosting/runtime configuration, end-to-end test coverage | Planned |

## Feature Backlog

| ID | Feature | Phase | Priority | Status | Acceptance Criteria |
|----|---------|-------|----------|--------|---------------------|
| F-001 | Scaffold the Minimal API host (.NET 10, Kestrel) with the vertical-slice project structure | 1 | Must | Planned | Service runs on Kestrel; folder/project convention established for one slice per payment type, ready for future payment types to be added alongside Direct Entry |
| F-002 | Wire up Wolverine as the in-process command/handler pipeline | 1 | Must | Planned | Wolverine is registered; a request flows end to end from the HTTP endpoint to a handler via Wolverine |
| F-003 | Implement the Direct Entry request/response contract (accept an array of payment instructions; return JSON with converted text or validation errors) | 1 | Must | Planned | Endpoint accepts the documented instruction array shape; returns the converted text inline in a JSON response on success |
| F-004 | Implement the Payment Type Router (dispatch by payment type; reject the whole batch if any instruction's type isn't supported yet) | 1 | Must | Planned | A batch containing any unsupported payment type is rejected in full, per FR-006 |
| F-005 | Implement Direct Entry validation rules (BSB format, account number rules, amount format, mandatory fields, etc., per the Direct Entry spec) | 1 | Must | Planned | Every field rule from the Direct Entry spec is enforced; an invalid batch is rejected in full with a reason per invalid instruction, per FR-002/FR-003 |
| F-006 | Implement Direct Entry detail record mapping (manual mapping, no AutoMapper) | 1 | Must | Planned | Each valid instruction maps to a detail record with correct field positions (BSB, account, indicator, transaction code, amount, title, lodgement reference, trace BSB/account, remitter name, withholding tax) |
| F-007 | Implement Direct Entry header and trailer record assembly, including self-balancing totals | 1 | Must | Planned | Header/trailer records populate per spec; trailer totals (credit, debit, net, record count) reconcile against the detail records |
| F-008 | Assemble the final fixed-width Direct Entry file content (120-character, CRLF-terminated records) and return it as text in the JSON response | 1 | Must | Planned | Output matches the Direct Entry spec's structural rules for a valid batch, per FR-004 and ADR-008 |
| F-009 | Integrate Shaw.Diagnostics logging (internal NuGet feed, v2.0.0) with redaction of sensitive payment fields | 2 | Must | Planned | Logs are emitted via Shaw.Diagnostics; no account numbers, amounts, or names appear in plaintext logs, per NFR-002/ADR-006 |
| F-010 | Confirm and document the trusted internal-only deployment boundary (no application-level auth) | 2 | Must | Planned | Deployment network boundary documented; service runs without app-level auth controls, per NFR-003 |
| F-011 | Define and validate batch size/performance targets | 2 | Should | Planned | Target batch size and latency confirmed (resolves Architecture A4) and validated under representative load |
| F-012 | Kestrel-only hosting/runtime configuration (no Docker, no database) | 3 | Must | Planned | Service runs directly on Kestrel in the target environment with no container runtime and no database dependency, per ADR-005 |
| F-013 | End-to-end test coverage for Direct Entry conversion (happy path and validation-failure paths) | 3 | Must | Planned | Automated tests cover: a valid batch converts correctly; a batch with one invalid instruction is rejected in full with reasons; a batch with an unsupported payment type is rejected |

## Dependencies

| Item | Depends On | Blocks | Notes |
|------|-----------|--------|-------|
| F-002 | F-001 | F-003 | Needs the host scaffolded first |
| F-003 | F-002 | F-004 | Needs Wolverine wired up |
| F-004 | F-003 | F-005 | Needs the request contract in place |
| F-005 | F-004 | F-006 | Needs routing before per-type validation |
| F-006 | F-005 | F-007 | Needs valid instructions before mapping |
| F-007 | F-006 | F-008 | Needs detail records before totals can be computed |
| F-008 | F-007 | F-013 | Completes the core conversion capability |
| F-009 | F-001 | — | Independent of the conversion logic itself |
| F-010 | F-001 | — | Independent of the conversion logic itself |
| F-011 | F-008 | — | Needs a working conversion path to measure against |
| F-012 | F-001 | F-013 | Needs the host in place before finalising hosting config |
| F-013 | F-008, F-012 | — | Needs the full conversion path and hosting config to test end to end |

## Out of Scope (confirmed in PRD)

- Payment types other than Direct Entry in this tranche (e.g. BPAY, international transfers) — planned as future tranches, not excluded permanently.
- Ingesting or handling CommBank return/status files, or feeding results back to Shaw and Partners — permanently out of scope (PRD Non-Goals).
- A general-purpose "any format to any format" conversion platform.
- Application-level authentication (service is internal-only).
- Persistent storage, a database, or containerization.

## Open Items

| ID | Item | Owner | Due |
|----|------|-------|-----|
| PM-001 | Future-tranche candidates from the document stash: BPAY (BPay Payments spec) and International/Priority Payments (MT101 spec) look like clear candidates for Tranche v2+, each converting to their own CommBank format. BAI2 and the DELIST CSV spec appear to be account-information/reporting formats rather than outbound payment types analogous to Direct Entry — confirm whether either is actually in scope before planning a tranche around them. BTRS Enriched and the Status Files/Naming Conventions spec relate to bank return/status reporting, which the PRD already excludes permanently — flagged here only so they aren't mistaken for a future payment-type tranche. | User | Before Tranche v2 planning |
| PM-002 | Architecture Open Question A4 (expected/maximum batch size and target conversion latency) is still open — feeds directly into F-011's acceptance criteria. | User | Before Phase 2 completion |

## Version History
| Version | Date | Change | Triggered By |
|---------|------|--------|---------------|
| v1 | 2026-08-13 | Initial draft | — |
