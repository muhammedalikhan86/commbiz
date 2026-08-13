# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0] — 2026-08-14

### Added

#### Self-Balancing Record Support
- **F-014:** Self-balancing (contra) detail record before trailer; minimum batch size reduced from 2 to 1
  - Supports single-instruction Direct Entry batches (previously rejected)
  - Every successful conversion now includes an additional self-balancing detail record before the trailer
  - Net total of all instructions balanced to zero in the self-balancing record
  - Output content (byte-for-byte) changes; JSON response shape remains unchanged
  - No breaking changes to request or response contracts

### Modules/Files Modified
- `src/CommBiz.Api/Features/DirectEntry/`
  - `DirectEntryValidator.cs` — Updated batch size minimum from 2 to 1
  - `DirectEntryTrailerRecordMapper.cs` — Updated to accommodate self-balancing record
  - `DirectEntrySelfBalancingRecordMapper.cs` [new] — Maps contra record with net-zero totals
  - `DirectEntryAmountTotals.cs` [new] — Calculates and stores credit/debit aggregates
  - `ConvertDirectEntryBatchCommand.cs` — Orchestrates self-balancing record insertion
  
- `tests/CommBiz.Api.Tests/`
  - `DirectEntryValidatorTests.cs` — Updated to verify single-instruction success
  - `DirectEntryTrailerRecordMapperTests.cs` — Updated for self-balancing record position
  - `DirectEntrySelfBalancingRecordMapperTests.cs` [new] — Unit tests for contra mapping
  - `ConvertDirectEntryBatchHandlerTests.cs` — Updated integration tests

### Breaking Changes
None — behavior change is backwards-compatible; single-instruction batches now succeed instead of failing.

### Test Coverage
**79 passing tests** (verified via `dotnet test`, 0 failed, 0 skipped):

1. **Happy Path — Multi-Instruction Batch with Self-Balancing Record**
   - POST valid 2+ instruction Direct Entry batch to `POST /convert`
   - Verify response includes self-balancing detail record immediately before trailer
   - Confirm self-balancing record net total is zero (credit_sum == debit_sum or vice versa)
   - Validate fixed-width text output embedding in JSON response

2. **Edge Case — Single-Instruction Batch Success**
   - POST batch with exactly 1 instruction (previously rejected at minimum-2 threshold)
   - Verify request now succeeds and includes self-balancing record
   - Confirm conversion pipeline handles minimal valid batch correctly

3. **Regression-Sensitive — Zero-Instruction Batch Still Rejected**
   - POST batch with zero instructions
   - Verify request is rejected with clear validation error (minimum is now 1, not 0)
   - Confirm minimum batch size validation remains in place

## [1.0.0] — 2026-08-13

### Added

#### Core Capabilities
- **F-001:** Scaffold the Minimal API host (.NET 10, Kestrel) with vertical-slice project structure
  - Enables fast startup and minimal dependencies
  - Foundation for feature-driven vertical slice architecture
  
- **F-002:** Wire up Wolverine as the in-process command/handler pipeline
  - Proof-of-pipeline integration via Diagnostics/Ping endpoint
  - Ready for CQRS/command pattern expansion in future features
  
- **F-003:** Implement the Direct Entry request/response contract
  - Shaw and Partners → CommBank Direct Entry batch DTOs
  - Structured JSON request/response envelope for batch conversion
  
- **F-004:** Implement the Payment Type Router
  - Routes incoming payment instructions by type (Direct Entry dispatcher ready)
  - Extensible pattern for future payment types (BPAY, international transfers, etc.)
  
- **F-005:** Implement Direct Entry validation rules
  - Batch-level constraints: minimum 2 instructions, maximum 10,000
  - Instruction-level validation: required fields, format constraints, valid codes
  - Early failure detection before submission to CommBank
  
- **F-006:** Implement Direct Entry detail record mapping (manual mapping, no AutoMapper)
  - Converts payment instructions to fixed-width Direct Entry detail records
  - Preserves account details, amounts, dates, and payment codes
  - No external mapping library dependency
  
- **F-007:** Implement Direct Entry header and trailer record assembly with self-balancing totals
  - Header records with batch identification and timestamp
  - Trailer records with self-balancing net totals (|credit - debit|)
  - Correct count of detail records included
  
- **F-008:** Assemble the final fixed-width Direct Entry file content and return as text in JSON response
  - Complete Direct Entry batch as RFC 80-column fixed-width text
  - Embedded in JSON response for easy integration with upstream systems
  - Format validated against CommBank Direct Entry specifications

### Modules/Files
- `src/CommBiz.Api/`
  - `Program.cs` — Minimal API host configuration, dependency injection, Wolverine pipeline setup
  - `appsettings.json` — Application configuration
  
- `src/CommBiz.Api/Features/Diagnostics/`
  - `Ping.cs` — Proof-of-pipeline endpoint demonstrating Wolverine integration
  
- `src/CommBiz.Api/Features/DirectEntry/`
  - `ConvertDirectEntryBatchCommand.cs` — CQRS command for batch conversion
  - `ConvertDirectEntryBatchRequest.cs` — Request DTO with batch and instruction payloads
  - `ConvertDirectEntryBatchResponse.cs` — Response DTO with converted file content
  - `ConvertDirectEntryBatchHandler.cs` — Command handler orchestrating conversion pipeline
  - `DirectEntryRouter.cs` — Payment type router dispatcher
  - `DirectEntryValidator.cs` — Batch and instruction-level validation rules
  - `DirectEntryDetailRecordMapper.cs` — Maps instructions to fixed-width detail records
  - `DirectEntryHeaderRecordMapper.cs` — Assembles header records
  - `DirectEntryTrailerRecordMapper.cs` — Assembles trailer records with self-balancing totals
  - `DirectEntryAssembler.cs` — Combines header, details, and trailer into final output
  
- `tests/CommBiz.Api.Tests/`
  - `ConvertDirectEntryBatchHandlerTests.cs` — Command handler integration tests
  - `DirectEntryConvertEndpointTests.cs` — HTTP endpoint contract tests
  - `DirectEntryDetailRecordMapperTests.cs` — Detail record mapping unit tests
  - `DirectEntryHeaderRecordMapperTests.cs` — Header record assembly unit tests
  - `DirectEntryTrailerRecordMapperTests.cs` — Trailer record and totals unit tests
  - `DirectEntryValidatorTests.cs` — Validation rule unit tests
  - `HealthEndpointTests.cs` — Health check endpoint tests
  - `PingEndpointTests.cs` — Wolverine proof-of-pipeline tests

### Breaking Changes
None — this is the initial release. No prior public API existed.

### Test Coverage
**106 passing tests** across:

1. **Happy Path — Valid Multi-Instruction Direct Entry Batch**
   - POST valid 2+ instruction Direct Entry batch to `/api/direct-entry/convert`
   - Verify response includes structurally correct Header, Detail, and Trailer records
   - Confirm fixed-width text output is embedded in JSON response
   - Validate CommBank format compliance (80-column fixed-width, RFC line ending)

2. **Edge Case — Minimum Instruction Threshold**
   - POST batch with exactly 1 instruction (below minimum-2-instruction rule)
   - Verify request is rejected with clear validation error
   - Confirm error response does not proceed to conversion

3. **Regression-Sensitive Flow — Self-Balancing Mixed Credit/Debit Batch**
   - POST batch with mixed credit and debit instructions
   - Verify Trailer record calculates correct net totals: net = |credit_sum - debit_sum|
   - Confirm detail record count matches instruction count
   - Validate all credit amounts are positive, all debit amounts are negative (or vice versa per convention)

### Known Limitations
- Direct Entry payment type only; BPAY and international transfers planned for Phase 2
- In-process command pipeline (Wolverine) only; no persistence or external queue integration yet
- No retry, dead-letter, or long-running job orchestration in this release
- CommBank bank return/reconciliation ingestion is out of scope; conversions are fire-and-forget

### Dependencies
- .NET 10 (Kestrel runtime only, no database or Docker)
- Wolverine 2.x (in-process CQRS command pipeline)
- Manual mapping (no AutoMapper, no third-party serialization overrides)

---

[1.0.0]: https://github.com/shaw-and-partners/commbiz/releases/tag/v1.0.0
