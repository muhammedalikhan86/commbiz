# Test Cases: Shaw and Partners → CommBank Payment File Conversion Service

> Status: DRAFT
> Version: v1
> Last updated: 2026-08-13
> Source: docs/project-management.md (Feature Backlog), docs/architecture.md (FR-001–FR-006),
> docs/stash/Direct Entry - File Specification CommBiz.md

Each scenario below is intended to be directly executable as an integration/E2E test against the
running Minimal API host. Field values reference the Direct Entry spec's Sample File section.

## Phase 1 — Direct Entry Conversion Core

### F-001/F-002 — Host + Wolverine wiring (smoke)

| ID | Scenario | Steps | Expected Result |
|----|----------|-------|------------------|
| TC-001 | Service starts and responds | Start the host; `GET`/`POST` a trivial request to the conversion endpoint | Service responds (not connection-refused); request reaches a Wolverine handler, confirmed via a log entry or a well-formed (even if empty-batch) response rather than a 404/500 routing failure |

### F-003/F-004 — Request contract + Payment Type Router

| ID | Scenario | Steps | Expected Result |
|----|----------|-------|------------------|
| TC-002 | Happy path — single valid Direct Entry instruction | POST a batch of 1 valid Direct Entry instruction (BSB `062-000`, account `10001000`, indicator `N`, txn code `53`, amount `530000010050` cents pattern per sample, title `CLIENT COMPANY XYZ`, lodgement ref `INVOICE 123456`) | 200 OK; JSON response contains the converted file text inline; success indicator true |
| TC-003 | Happy path — multiple valid instructions (regression-sensitive: totals) | POST a batch of 3+ valid instructions with mixed credit/debit transaction codes | 200 OK; converted file contains 1 header + 3 detail + 1 trailer record; trailer totals reconcile (net = credit − debit, counts match) |
| TC-004 | Unsupported payment type rejects whole batch | POST a batch where one instruction declares a payment type other than Direct Entry (e.g. `BPAY`), alongside otherwise-valid Direct Entry instructions | 4xx response; entire batch rejected (no partial conversion); reason references the unsupported type, per FR-006 |
| TC-005 | Mixed-type batch — all instructions must be same/supported type | POST a batch with only unsupported types | 4xx response; batch rejected; no output produced |

### F-005 — Direct Entry validation rules

| ID | Scenario | Steps | Expected Result |
|----|----------|-------|------------------|
| TC-006 | Invalid BSB format rejected | POST a batch with one instruction having a malformed BSB (e.g. `62000` missing hyphen/digit) | 4xx; batch rejected in full; validation reason for that instruction cites BSB format, per FR-002/FR-003 |
| TC-007 | Account number all-zero/blank rejected | POST an instruction with account number `00000000` or all-blank | 4xx; batch rejected; reason cites account number rule |
| TC-008 | Invalid indicator value rejected | POST an instruction with indicator outside `N`/`W`/`X`/`Y`/blank (e.g. `Z`) | 4xx; batch rejected; reason cites indicator rule |
| TC-009 | Invalid transaction code rejected | POST an instruction with a transaction code outside the documented set (e.g. `99`) | 4xx; batch rejected; reason cites transaction code rule |
| TC-010 | Non-numeric amount rejected | POST an instruction with a non-numeric or negative amount | 4xx; batch rejected; reason cites amount format |
| TC-011 | Mandatory field missing (title) rejected | POST an instruction with a blank "Title of Account" field | 4xx; batch rejected; reason cites mandatory field |
| TC-012 | One invalid instruction among many valid ones rejects the whole batch (regression-sensitive) | POST a batch of 5 valid instructions + 1 invalid instruction | 4xx; batch rejected in full with a reason for the one invalid instruction; the 5 valid instructions are NOT partially converted, per FR-003 |

### F-006 — Detail record mapping

| ID | Scenario | Steps | Expected Result |
|----|----------|-------|------------------|
| TC-013 | Detail record field positions match spec | Convert one valid instruction; inspect output at fixed character offsets | Record type `1` at pos 1; BSB at 2-8; account at 9-17; indicator at 18; txn code at 19-20; amount (10 digits, zero-filled) at 21-30; title (left-justified, space-filled) at 31-62; lodgement ref at 63-80; trace BSB at 81-87; trace account at 88-96; remitter name at 97-112; withholding tax at 113-120 |
| TC-014 | Amount right-justified, zero-filled | Convert an instruction with a small amount (e.g. $1.00 = 100 cents) | Amount field is exactly 10 characters, zero-filled, e.g. `0000000100` |
| TC-015 | Title left-justified, space-filled, truncation/no-truncation boundary | Convert an instruction with a title at exactly 32 chars and one under 32 chars | Field is exactly 32 characters wide; short titles are space-padded on the right |

### F-007 — Header/trailer assembly + self-balancing totals

| ID | Scenario | Steps | Expected Result |
|----|----------|-------|------------------|
| TC-016 | Header record fields per spec | Convert any valid batch; inspect header record | Record type `0`; reel sequence `01`; "CBA" at 21-23; date processed in DDMMYY format at 75-80; all blank-filled sections are space-filled |
| TC-017 | Trailer self-balancing — credit only | Convert a batch of only credit transactions | Trailer: net total = credit total; debit total = `0000000000`; record count matches detail count |
| TC-018 | Trailer self-balancing — mixed credit/debit (regression-sensitive) | Convert a batch with both credit and debit transaction codes | Trailer: net = credit − debit (unsigned); credit total = sum of credit detail amounts; debit total = sum of debit detail amounts; BSB placeholder `999-999` |

### F-008 — Final file assembly

| ID | Scenario | Steps | Expected Result |
|----|----------|-------|------------------|
| TC-019 | Full file structural validation (happy path, E2E) | Convert a batch of 2+ valid instructions | Output: 1 header + N detail + 1 trailer records; each record exactly 120 characters (or 80 with trailing optional fields dropped, per spec); every record CRLF-terminated; matches Sample File structure in the Direct Entry spec |
| TC-020 | Response contract on success | Convert a valid batch | JSON response contains converted text inline (no download link, per ADR-008) and a success indicator |

## Phase 2 — Cross-Cutting Concerns (F-009, F-010, F-011)

| ID | Scenario | Steps | Expected Result |
|----|----------|-------|------------------|
| TC-021 | No sensitive data in logs | Convert a batch containing account numbers, amounts, and names; inspect emitted logs | No account numbers, amounts, or names appear in plaintext in any log entry, per NFR-002/ADR-006 |
| TC-022 | No app-level auth enforced | Call the endpoint with no credentials/headers | Request is processed normally (network-boundary trust model, per NFR-003) — confirms no auth middleware blocks it |
| TC-023 | Batch size / latency target (once A4 resolved) | Convert a batch at the confirmed target size | Conversion completes within the target latency (PM-002 — currently blocked on A4 resolution) |

## Phase 3 — Release Readiness (F-012, F-013)

| ID | Scenario | Steps | Expected Result |
|----|----------|-------|------------------|
| TC-024 | Kestrel-only hosting (regression-sensitive) | Start the service in the target environment | Runs directly on Kestrel; no container runtime process; no database connection attempted |
| TC-025 | E2E — happy path | Full request → valid batch → response | Converted file returned inline, structurally valid |
| TC-026 | E2E — validation failure path | Full request → batch with 1 invalid instruction | Batch rejected in full, reasons returned |
| TC-027 | E2E — unsupported payment type path | Full request → batch with unsupported type | Batch rejected in full |

## Version History
| Version | Date | Change | Triggered By |
|---------|------|--------|---------------|
| v1 | 2026-08-13 | Initial draft, derived from PMBook Feature Backlog and Direct Entry spec | Orchestrator Step 0.0 |
