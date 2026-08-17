# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.3.0] — 2026-08-17

### Added

#### Shared Field Mapping Model & Response Enrichment
- **F-021:** Unified `FieldMapping` and `LineMapping` model suite for all payment types
  - New `Mappings` field in all conversion responses (Direct Entry, BPAY, IMT)
  - Per-line request vs. CBA-spec breakdown for testing and audit trails
  - Ordered parallel to `ConvertedText` for precise field traceability
  - Covers field extraction, validation results, and mapping decisions per instruction
  
- **F-021 Follow-up Refactor:** Extracted duplicated field/width helpers to `Features/Shared/MappingUtilities.cs`
  - `AmountToCents()` and `FixedWidth()` utilities now shared across all three payment type mappers
  - Zero behavior change; pure internal consolidation

### Modules/Files Modified
- `src/CommBiz.Api/Features/Shared/`
  - `FieldMapping.cs` [new] — Defines FieldMapping model for individual field mapping metadata
  - `LineMapping.cs` [new] — Defines LineMapping model for per-line aggregation
  - `MappingUtilities.cs` [new] — Extracted AmountToCents and FixedWidth helpers
  
- `src/CommBiz.Api/Features/DirectEntry/`
  - `ConvertDirectEntryBatchHandler.cs` — Added Mappings field to response
  - `DirectEntryMapper.cs` — Enhanced to populate LineMapping for each line
  - `ConvertDirectEntryBatchCommand.cs` — Response shape updated
  
- `src/CommBiz.Api/Features/BPay/`
  - `ConvertBPayBatchHandler.cs` — Added Mappings field to response
  - `BPayDetailRecordMapper.cs` — Enhanced to populate LineMapping for each detail
  - `ConvertBPayBatchCommand.cs` — Response shape updated
  
- `src/CommBiz.Api/Features/Imt/`
  - `ConvertImtBatchHandler.cs` — Added Mappings field to response
  - `ImtCsvMapper.cs` — Enhanced to populate LineMapping for each CSV row
  - `ConvertImtBatchCommand.cs` — Response shape updated
  
- `tests/CommBiz.Api.Tests/`
  - `DirectEntry/ConvertDirectEntryBatchHandlerTests.cs` — Updated to verify Mappings population
  - `BPay/ConvertBPayBatchHandlerTests.cs` — Updated to verify Mappings population
  - `Imt/ConvertImtBatchHandlerTests.cs` — Updated to verify Mappings population

### Breaking Changes
None — `Mappings` field is purely additive to all existing response contracts; no modification to existing fields or request shapes.

### Test Coverage
**244 passing tests** (verified via `dotnet test`, 0 failed, 0 skipped; 0 vulnerabilities):

1. **Happy Path — Multi-Instruction Batch with Populated Mappings**
   - POST valid multi-instruction Direct Entry, BPAY, or IMT batch to `POST /convert`
   - Verify response includes `ConvertedText` (existing, unchanged) AND `Mappings` array (new, populated)
   - Confirm `Mappings` is ordered and parallel to `ConvertedText` line-by-line
   - Each LineMapping includes per-field request vs. CBA-spec breakdown and mapping decisions
   
2. **Edge Case — Single-Instruction Batch Mappings**
   - POST Direct Entry batch with exactly 1 instruction
   - Verify `Mappings` array contains 1 LineMapping
   - Confirm LineMapping does NOT include `detail2` entry (single-line scenario)
   - Validate Mappings structure matches schema for single-instruction case

3. **Regression-Sensitive — Validation Failure Mappings Null**
   - POST batch with validation error (e.g., invalid instruction code)
   - Verify response has `Mappings: null` for all three payment slices
   - Confirm `ConvertedText: null` and error structure unchanged
   - POST same instruction set with F-001–F-020 behavior (pre-F-021)
   - Verify output byte-for-byte identical to Revision 3 for all passing scenarios

## [1.2.0] — 2026-08-17

### Added

#### Payment Type Router Extension — Cross-Slice Dispatcher
- **F-015:** Payment Type Router promoted from static endpoint pattern to a real, extensible top-level dispatcher
  - Routes all payment types through a unified command pipeline
  - Integrates Direct Entry, BPAY, and IMT into a single request/response surface
  - Maintains backward compatibility with existing Direct Entry behavior

#### BPAY Batch Payments Conversion
- **F-016:** BPAY batch conversion pipeline (Header + Details only, no trailer)
  - Validator enforces BPAY-specific batch constraints and instruction rules
  - `BPayHeaderRecordMapper` converts batch metadata to BPAY fixed-width header
  - `BPayDetailRecordMapper` converts individual payment instructions to detail records
  - Handler orchestrates validation, mapping, and assembly into final BPAY file content
  - Ready for trailer record addition in future phases

#### International Money Transfers (IMT) Conversion
- **F-017:** IMT batch conversion pipeline (27-field MT101-family CSV format)
  - Supports 27-field MT101-derived CSV structure for international money transfers
  - Implements SWIFT country code derivation from payment instruction data
  - Reject-vs-sanitize field handling: invalid fields rejected at validation vs. sanitized at mapping per field spec
  - CSV header and detail rows with structured field ordering for banking system compliance

### Modules/Files Modified
- `src/CommBiz.Api/`
  - `Program.cs` — Registered BPAY and IMT handlers into Wolverine command pipeline
  
- `src/CommBiz.Api/Features/PaymentRouting/`
  - `PaymentTypeRouter.cs` [new] — Central dispatcher routing to DE/BPAY/IMT handlers
  - `DispatchPaymentTypeCommand.cs` [new] — Command to route payment type selection
  - `DispatchPaymentTypeHandler.cs` [new] — Handler executing router logic
  
- `src/CommBiz.Api/Features/BPay/`
  - `BPayConvertCommand.cs` [new] — Command encapsulating BPAY batch conversion request
  - `BPayValidator.cs` [new] — Batch and instruction-level validation rules
  - `BPayHeaderRecordMapper.cs` [new] — Maps batch metadata to fixed-width header
  - `BPayDetailRecordMapper.cs` [new] — Maps instruction to detail record
  - `ConvertBPayBatchHandler.cs` [new] — Orchestrates BPAY conversion pipeline
  
- `src/CommBiz.Api/Features/Imt/`
  - `ImtConvertCommand.cs` [new] — Command encapsulating IMT batch conversion request
  - `ImtValidator.cs` [new] — Batch and instruction-level validation rules
  - `ImtCsvMapper.cs` [new] — Maps batch to 27-field MT101-derived CSV
  - `ImtCountryCodeResolver.cs` [new] — Derives SWIFT country codes from instruction data
  - `ConvertImtBatchHandler.cs` [new] — Orchestrates IMT conversion pipeline
  
- `tests/CommBiz.Api.Tests/`
  - `PaymentRouting/DispatchPaymentTypeHandlerTests.cs` [new] — Router dispatch logic tests
  
  - `BPay/BPayConvertEndpointTests.cs` [new] — Full integration tests (POST endpoint)
  - `BPay/BPayValidator.cs` [new] — Validator constraint tests
  - `BPay/BPayHeaderRecordMapperTests.cs` [new] — Header mapping tests
  - `BPay/BPayDetailRecordMapperTests.cs` [new] — Detail record mapping tests
  - `BPay/ConvertBPayBatchHandlerTests.cs` [new] — Handler orchestration tests
  
  - `Imt/ImtConvertEndpointTests.cs` [new] — Full integration tests (POST endpoint)
  - `Imt/ImtValidator.cs` [new] — Validator constraint tests
  - `Imt/ImtCsvMapperTests.cs` [new] — CSV mapping and field ordering tests
  - `Imt/ImtCountryCodeResolverTests.cs` [new] — Country code derivation tests
  - `Imt/ConvertImtBatchHandlerTests.cs` [new] — Handler orchestration tests

### Breaking Changes
None — Direct Entry behavior unchanged; BPAY and IMT are net-new payment types extending the existing API surface without modifying existing routes or contracts.

### Test Coverage
**206 passing tests** (verified via `dotnet test`, 0 failed, 0 skipped):

1. **Happy Path — BPAY Header + Details Assembly**
   - POST valid BPAY batch (2+ instructions) to `POST /convert` with BPAY payment type
   - Verify response includes correctly formatted fixed-width BPAY header and detail records
   - Confirm batch amount totals and instruction count match header
   - Validate response JSON structure unchanged from Direct Entry contract

2. **Happy Path — IMT 27-Field CSV Generation**
   - POST valid IMT batch (2+ instructions) to `POST /convert` with IMT payment type
   - Verify response includes CSV header row and correctly ordered 27-field detail rows
   - Confirm SWIFT country codes resolved correctly from payment instruction data
   - Validate CSV field ordering and character encoding compliance

3. **Edge Case — BPAY Single-Instruction Batch Success**
   - POST batch with exactly 1 BPAY instruction
   - Verify request succeeds and includes header + single detail record
   - Confirm self-balancing record placement follows BPAY format rules

4. **Edge Case — IMT Field Rejection vs. Sanitization**
   - POST IMT batch with invalid values in reject-on-error fields
   - Verify validation fails with clear field error message
   - POST same batch with invalid values in sanitize-on-error fields
   - Verify request succeeds and sanitized values appear in output

5. **Regression-Sensitive — Direct Entry Behavior Unchanged**
   - POST Direct Entry batch via existing route
   - Confirm output, self-balancing record, and response structure identical to Revision 2
   - Verify router correctly dispatches DE instructions without modification

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
