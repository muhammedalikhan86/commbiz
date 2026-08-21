# Test Cases: Shaw and Partners → CommBank Payment File Conversion Service

> Status: DRAFT
> Version: v4
> Last updated: 2026-08-18
> Source: docs/project-management.md (Feature Backlog), docs/architecture.md (FR-001–FR-008),
> docs/stash/Direct Entry - File Specification CommBiz.md, docs/stash/BPay Payments - CommBiz File
> Specification.md, docs/stash/CommBiz File Specification - International Money Transfers Priority
> Payments Non CBA Payment Requests (MT101) v9.md

Each scenario below is intended to be directly executable as an integration/E2E test against the
running Minimal API host. Field values reference the Direct Entry spec's Sample File section.

> **Editorial note (PM-003, still open — see docs/project-management.md):** every scenario below
> described as a rejection is worded as returning a "4xx response". The current implementation
> actually returns `200 OK` with a `success: false` JSON envelope for every rejection — the status
> code itself is never 4xx; rejection is signalled only in the response body. This is a still-open
> product decision (PM-003), not resolved by this update — the per-row "4xx response" wording below
> is left as-is rather than silently rewritten. See
> docs/testing/phase-1-direct-entry-conversion-core.md's "⚠️ Known caveat" section for the corrected
> pattern to apply once PM-003 is resolved.

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
| TC-004 | Mixed *recognised* payment types in one batch rejects the whole batch (distinct from TC-005) | POST a batch mixing Direct Entry (`DE`) instructions with a different, recognised payment type (e.g. `BPAY`) | 4xx response; entire batch rejected (no partial conversion) with exactly one error at index `-1`; reason is the batch-level "must not mix payment types" message, e.g. `Payment batch must not mix payment types (found 'DE', 'BPAY').`, per FR-006 |
| TC-005 | Every instruction shares one *unsupported* payment type rejects the whole batch (distinct from TC-004) | POST a batch where every instruction declares the same not-yet-wired payment type (e.g. `PP`) | 4xx response; batch rejected; one error per instruction, each citing that instruction's 0-based index and the message `Unsupported payment type 'PP'.`; no output produced. This is a per-instruction, unrecognised-type rejection — not the batch-level mixed-type rejection in TC-004 |

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

### F-014 — Self-balancing (contra) detail record

| ID | Scenario | Steps | Expected Result |
|----|----------|-------|------------------|
| TC-028 | Single-instruction batch now converts successfully (regression-sensitive) | POST a batch of exactly 1 valid Direct Entry instruction | 200 OK; output contains 1 header + 1 real detail + 1 self-balancing detail + 1 trailer record; previously rejected under the old minimum-2 rule |
| TC-029 | Zero-instruction batch is still rejected | POST an empty batch | 4xx; batch rejected; reason cites the minimum-1-instruction rule |
| TC-030 | Self-balancing detail record fields per spec | Convert any valid batch; inspect the detail record immediately before the trailer | Record type `1`; account (pos 2-17) is the configured settlement account (`TraceAccountBsb`/`TraceAccountAccNo`); Transaction Code (pos 19-20) is the inverse of the batch's configured `TransactionCode`; Amount (pos 21-30) equals the sum of all real detail record amounts, in cents; Withholding Tax (pos 113-120) is zero |
| TC-031 | Self-balancing record positioned immediately before the trailer (regression-sensitive) | Convert a batch of 3+ valid instructions | Output order is: header, 1 detail record per instruction, then the self-balancing detail record, then the trailer — never after the trailer or interleaved with real details |
| TC-032 | Trailer reconciles to zero net once the self-balancing record is included (regression-sensitive) | Convert any valid batch; inspect trailer | Net total = `0000000000`; credit total = debit total = sum of real detail amounts; File (User) Count of Record Type 1 includes the self-balancing record (real instruction count + 1) |

### F-008 — Final file assembly

| ID | Scenario | Steps | Expected Result |
|----|----------|-------|------------------|
| TC-019 | Full file structural validation (happy path, E2E) | Convert a batch of 2+ valid instructions | Output: 1 header + N detail + 1 self-balancing detail + 1 trailer records; each record exactly 120 characters (or 80 with trailing optional fields dropped, per spec); every record CRLF-terminated; matches Sample File structure in the Direct Entry spec |
| TC-020 | Response contract on success | Convert a valid batch | JSON response contains converted text inline (no download link, per ADR-008) and a success indicator |

## Phase 2 — Additional Payment Types (F-015–F-018)

> Covers F-015 (Payment Type Router extended to BPAY/IMT), F-016 (BPAY Batch Payments), F-017 (IMT).
> F-018 (Priority Payments) is still Planned (blocked on PM-006) and has no scenarios here yet.
> F-022/F-023 (FX conversion) now has scenarios below (PM-012 closure) — this is a separate addition
> and does not resolve F-018's still-open gap (tracked separately under PM-010).

### F-015 — Payment Type Router: additional dispatch/rejection paths (beyond TC-004/TC-005)

| ID | Scenario | Steps | Expected Result |
|----|----------|-------|------------------|
| TC-033 | Empty batch rejected before type can be determined | POST an empty array (`[]`) | 4xx response; single error at index `-1`, reason `Payment batch must contain at least 1 payment instruction; unable to determine payment type.` — this router-level message is distinct from a slice validator's own "at least 1 payment instruction(s) (found 0)" minimum-count message, since the router never reaches a slice validator when the type can't be determined |
| TC-034 | Direct Entry dispatch is case-insensitive | POST a valid single-instruction DE batch with `paymentTypeCode: "de"` (lower-case) | 200 OK; `success: true`; routes to the Direct Entry slice identically to `"DE"` |
| TC-035 | BPAY dispatch is case-insensitive | POST a valid single-instruction BPAY batch with `paymentTypeCode: "bpay"` (lower-case) | 200 OK; `success: true`; routes to the BPAY slice identically to `"BPAY"` |
| TC-036 | IMT dispatch is case-insensitive | POST a valid single-instruction IMT batch with `paymentTypeCode: "tt"` (lower-case) | 200 OK; `success: true`; routes to the IMT slice identically to `"TT"` (Shaw and Partners' internal Telegraphic Transfer code, mapped to CBA's "IMT" file format) |

### F-016 — BPAY Batch Payments conversion

| ID | Scenario | Steps | Expected Result |
|----|----------|-------|------------------|
| TC-037 | Happy path — single valid BPAY instruction | POST a batch of 1 valid BPAY instruction (numeric `BPayBillerCode` ≤10 digits, numeric `BPayReference` ≤20 digits, positive `Amount` ≤9,999,999,999.99) | 200 OK; `success: true`; `convertedText` contains 1 Header record + 1 Payment Details record, no trailer |
| TC-038 | Happy path — multiple valid BPAY instructions (regression-sensitive) | POST a batch of 3+ valid BPAY instructions | 200 OK; `success: true`; `convertedText` contains 1 Header record + N Payment Details records (one per instruction), CRLF-terminated, no trailer/self-balancing record (not part of the BPay spec) |
| TC-040 | `BPayBillerCode` invalid rejected | POST an instruction with `BPayBillerCode` containing a non-digit character or exceeding 10 digits | 4xx response; batch rejected in full; error reason cites `BPayBillerCode` must be numeric, 1-10 digits |
| TC-041 | `BPayReference` invalid rejected | POST an instruction with `BPayReference` containing a non-digit character or exceeding 20 digits | 4xx response; batch rejected in full; error reason cites `BPayReference` must be numeric, 1-20 digits |
| TC-042 | `Amount` invalid rejected | POST an instruction with `Amount` ≤0 or >9,999,999,999.99 | 4xx response; batch rejected in full; error reason cites `Amount` must be positive and convert to at most 12 digits of cents |
| TC-061 | `PaymentDate` outside the processing window rejected | POST an instruction with `PaymentDate` in the past, or more than 15 months ahead | 4xx response; batch rejected in full; error reason cites `PaymentDate` must be between today and 15 months ahead |
| TC-043 | Max-200-instruction boundary (regression-sensitive) | POST a batch of exactly 200 valid instructions, then a batch of 201 | 200 instructions: 200 OK, `success: true`, all 200 convert. 201 instructions: 4xx response, batch rejected, error reason cites "at most 200 payment instruction(s) (found 201)" per the BPay spec's 200-payment file limit |

### F-017 — International Money Transfers (IMT) conversion

| ID | Scenario | Steps | Expected Result |
|----|----------|-------|------------------|
| TC-044 | Happy path — valid IMT instruction (regression-sensitive) | POST a batch of 1+ valid IMT instructions (valid SWIFT-derived `DestinationBankSwiftCode`, exactly one of `SourceAmount`/`Amount` populated, `PaymentDate` within today..+7 days, non-blank `Notes`/`BeneficiaryAddress`/`PaymentReference`) | 200 OK; `success: true`; `convertedText` is a 27-field-per-row CSV, CRLF between rows, no trailing CRLF, no trailer record |
| TC-045 | `Notes` (Transaction Description) blank rejected | POST an instruction with `Notes` blank/whitespace | 4xx response; batch rejected in full; error reason `Notes (Transaction Description) must not be blank.` |
| TC-046 | `PaymentDate` outside the processing window rejected | POST an instruction with `PaymentDate` in the past, or more than 7 days ahead | 4xx response; batch rejected in full; error reason cites `PaymentDate` must be between today and today + 7 days |
| TC-047 | `SourceCurrency` invalid format rejected | POST an instruction with `SourceCurrency` not exactly 3 upper-case letters (e.g. `aud` or `AU`) | 4xx response; batch rejected in full; error reason cites `SourceCurrency` must be exactly 3 upper-case letters |
| TC-048 | Exactly-one-of Payment/Debit Amount rule violated rejected | POST an instruction with both `SourceAmount` and `Amount` populated (>0), then a separate instruction with neither populated | 4xx response for both cases; batch rejected in full; error reason states exactly one of `SourceAmount` (Payment Amount) or `Amount` (Debit Amount) must be greater than zero |
| TC-049 | `DestinationBankSwiftCode` invalid length rejected | POST an instruction with `DestinationBankSwiftCode` not 8 or 11 alphanumeric characters | 4xx response; batch rejected in full; error reason cites `DestinationBankSwiftCode` must be 8 or 11 alphanumeric characters |
| TC-050 | Max-350-transaction boundary (regression-sensitive) | POST a batch of exactly 350 valid instructions, then a batch of 351 | 350 instructions: 200 OK, `success: true`, all 350 convert. 351 instructions: 4xx response, batch rejected, error reason cites "at most 350 payment instruction(s) (found 351)" per the IMT spec's 350-transaction file limit |

### F-022/F-023 — FX conversion

| ID | Scenario | Steps | Expected Result |
|----|----------|-------|------------------|
| TC-051 | Happy path — single valid FX instruction | POST a batch of 1 valid FX instruction (`buyCurrency`/`sellCurrency` exactly 3 uppercase letters, e.g. `USD`/`AUD`; positive `amount` with at most 11 integer digits and 2 decimal digits; `accountNo` 1-12 alphanumeric characters) | 200 OK; `success: true`; `convertedText` is a single 27-field CSV row starting with the constant `"FX,"` (never the API routing code `"FOREX,"`), no header/trailer record |
| TC-052 | FOREX dispatch is case-insensitive (mirroring TC-034/TC-035/TC-036) | POST a valid single-instruction FX batch with `paymentTypeCode: "forex"` (lower-case) | 200 OK; `success: true`; routes to the FX slice identically to `"FOREX"` |
| TC-053 | Happy path — multiple valid FX instructions, distinct currency pairs (regression-sensitive) | POST a batch of 2+ valid FX instructions, each with a different `buyCurrency`/`sellCurrency` pair | 200 OK; `success: true`; `convertedText` contains one 27-field CSV row per instruction, CRLF-separated, no trailing CRLF, no header/trailer record; `Mappings` populated with one entry per row (`row1`, `row2`, ...), each with 27 field entries |
| TC-054 | `BuyCurrency`/`SellCurrency` invalid format rejected | POST an instruction with `buyCurrency` or `sellCurrency` not exactly 3 uppercase alphabetic characters (e.g. lower-case `"usd"` or wrong-length `"US"`) | 4xx response; batch rejected in full; error reason cites `BuyCurrency`/`SellCurrency` must be exactly 3 uppercase alphabetic characters |
| TC-055 | `Amount` invalid format rejected | POST an instruction with `amount` ≤0, more than 11 integer digits, or more than 2 decimal digits (e.g. `100.123`) | 4xx response; batch rejected in full; error reason cites `Amount` must be greater than zero, with at most 11 integer digits and 2 decimal digits |
| TC-056 | `AccountNo` invalid rejected | POST an instruction with `accountNo` blank or exceeding 12 alphanumeric characters | 4xx response; batch rejected in full; error reason cites `AccountNo` must be 1-12 alphanumeric characters |
| TC-057 | Max-200-instruction boundary (regression-sensitive) | POST a batch of exactly 200 valid instructions sharing one currency pair, then a batch of 201 | 200 instructions: 200 OK, `success: true`, all 200 convert. 201 instructions: 4xx response, batch rejected, error reason cites "at most 200 payment instruction(s) (found 201)" per the FX file's 200-row limit |
| TC-058 | Max-15-distinct-currency-pair boundary (regression-sensitive) | POST a batch of 15 valid instructions, each with a distinct `buyCurrency` against a constant `sellCurrency`, then a batch of 16 distinct pairs | 15 pairs: 200 OK, `success: true`. 16 pairs: 4xx response, batch rejected, error reason cites "must settle at most 15 distinct currency pairs (found 16)" per the FX spec's 15-currency-pair limit |
| TC-059 | Mixed *recognised* payment types in one batch rejects the whole batch (mirroring TC-004) | POST a batch mixing FX (`FOREX`) instructions with a different, recognised payment type (e.g. `DE`) | 4xx response; entire batch rejected (no partial conversion) with exactly one error at index `-1`; reason is the batch-level "must not mix payment types" message, e.g. `Payment batch must not mix payment types (found 'FOREX', 'DE').`, per FR-006 |
| TC-060 | Response contract — `Mappings` present, no `Notes` field on the request at all (distinctive FX behaviour) | Convert a valid FX batch; inspect the response | `Mappings` populated with 27 field-position entries per row (`Transaction Type` through `Social Security Number (SSN)`); unlike every other payment type in this doc, `FxPaymentInstructionRequest` has no `Notes` field at all — it was removed as dead (never read by validation or mapping) rather than left as an unused pass-through field |

## Phase 3 — Cross-Cutting Concerns (F-009, F-010, F-011)

| ID | Scenario | Steps | Expected Result |
|----|----------|-------|------------------|
| TC-021 | No sensitive data in logs | Convert a batch containing account numbers, amounts, and names; inspect emitted logs | No account numbers, amounts, or names appear in plaintext in any log entry, per NFR-002/ADR-006 |
| TC-022 | No app-level auth enforced | Call the endpoint with no credentials/headers | Request is processed normally (network-boundary trust model, per NFR-003) — confirms no auth middleware blocks it |
| TC-023 | Batch size / latency target (once A4 resolved) | Convert a batch at the confirmed target size | Conversion completes within the target latency (PM-002 — currently blocked on A4 resolution) |

## Phase 4 — Release Readiness (F-012, F-013)

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
| v2 | 2026-08-13 | Added F-014 scenarios (self-balancing contra detail record, minimum batch size reduced to 1); updated TC-019 to expect the self-balancing detail record in the output | User requirement change |
| v3 | 2026-08-17 | Retroactive documentation catch-up for F-015/F-016/F-017 (already Done, Reviewer-PASS'd): added an editorial note flagging the still-open PM-003 status-code discrepancy (4xx wording below vs. actual `200 OK` + `success: false`); corrected TC-004/TC-005 to reflect the router's two distinct rejections (batch-level mixed-type at index -1 vs. per-instruction unsupported-type); renumbered stale "Phase 2 — Cross-Cutting"/"Phase 3 — Release Readiness" headers to the current PMBook v11 numbering (Phase 3/Phase 4); added new "Phase 2 — Additional Payment Types (F-015–F-018)" section, TC-033–TC-050, covering F-015 routing edge cases, F-016 BPAY, and F-017 IMT | Retroactive Finalizer documentation catch-up |
| v4 | 2026-08-18 | Added F-022/F-023 (FX conversion) scenarios, TC-051–TC-060, under Phase 2 — Additional Payment Types: happy path, FOREX dispatch case-insensitivity, multi-instruction batch, invalid `BuyCurrency`/`SellCurrency`/`Amount`/`AccountNo` rejections, 200-instruction and 15-currency-pair boundary tests, mixed-payment-type rejection, and the `Mappings`-present/`Notes`-never-output response-contract behaviour; appended a Phase 2 header note pointing to the new FX section (F-018/Priority-Payments wording, tracked separately under PM-010, left untouched); added `tests/smoke/Fx.http` | PM-012 closure — User requirement |
| v5 | 2026-08-21 | Added TC-061 (BPay `PaymentDate` processing-window validation, mirroring IMT/Priority Payments' own `PaymentDate` window checks) to the F-016 section; removed the dead, never-validated/never-mapped `PaymentSourceTypeCode` field from every slice's request shape and its test/doc references | User-directed cleanup |
| v6 | 2026-08-21 | Removed further dead request fields with zero validation/mapping usage: DirectEntry's `SourceBankAccountName`/`SourceCurrency`/`SourceAmount`/`CreateBy`; Priority Payments' `SourceBankAccountName`/`SourceBankAccountNo`/`SourceBankBsb`/`SourceCurrency`/`SourceAmount`; IMT's `SourceBankAccountName`/`SourceBankAccountNo`/`SourceBankBsb`/`DestinationBankTypeCode`/`Currency`/`DestinationBankIBAN`/`DestinationBankAddress`/`IntermediaryBankIBAN`/`IntermediaryBankAddress`; and FX's `PaymentDate`/`Notes`/`RateTypeCode`/`ValueDateTypeCode`/`FeeTypeCode`/`FeeOtherTypeCode`; updated TC-060 accordingly | User-directed cleanup |
| v7 | 2026-08-21 | Removed BPay's `AccountNo` and DirectEntry's `AccountNo`/`SourceBankBsb`/`SourceBankAccountNo` — the "validated but never mapped" fields deliberately kept during the v6 sweep, re-reviewed and found to have no output-mapping justification; TC-037 updated (dropped the `AccountNo` mention), TC-039 (`AccountNo` blank rejected) removed since the field and its validation no longer exist | User-directed re-review of validated-but-unmapped fields |
