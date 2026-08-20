# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.6.0] — 2026-08-20

### Fixed

#### Direct Entry Field-Mapping Correctness (Amended F-003/F-005/F-006/F-014)
- **Critical Bug Fix:** Direct Entry Detail Record's primary BSB/Account Number/Title fields now correctly sourced from three new required request fields (`DestinationBankBsb`, `DestinationBankAccountNo`, `DestinationBankAccountName` — the actual beneficiary account), instead of from the organisation's own Trace settings
  - **Impact:** Payments now correctly credit/debit the intended destination account. Previously, Detail Record was incorrectly populated from Trace settings, meaning payments were not crediting the intended beneficiary — now verified against real CommBank test accounts
  - Organisation's `TraceAccountBsb`/`TraceAccountAccNo`/`NameOfRemitter` now correctly populate only the Trace section
  - Transaction Code hardcoded to `"50"` (credit) for every detail record (removed configurable `TransactionCode` setting)
  - Self-balancing (contra) record now uses three new dedicated settings (`SelfBalancingAccountNo`, `SelfBalancingNameOfRemitter`, `SelfBalancingLodgementReferenceDetails`) with hardcoded `"13"` (debit) transaction code and hardcoded Title constant
  - Header record's `InstitutionCode`/`UserIdentificationNumber`/`NameOfUserSupplyingFile` moved from configuration to hardcoded mapper constants
  - Indicator changed from `"N"` → `" "` for both Detail and Self-Balancing records
  - `DirectEntryValidator` enhanced to validate the three new Destination* fields identically to Source*

### Added

#### New `/convert-to-file` Endpoint (Temporary)
- **F-025 (Informal):** New `POST /convert-to-file` endpoint returns payment conversion's `ConvertedText` as a downloadable `.txt` file instead of inline JSON
  - Reuses existing `PaymentTypeRouter` dispatch across all five payment types (Direct Entry, BPAY, IMT, Priority Payments, FX)
  - Falls back to standard JSON error envelope on any rejection/failure
  - Explicitly marked temporary in code; no product decision yet on permanence

### Changed

#### Breaking: New Required Direct Entry Request Fields
- `PaymentInstructionRequest` (the `/convert` and `/convert-to-file` request contract for `"DE"`-typed instructions) now requires three additional fields:
  - `destinationBankBsb` — beneficiary bank BSB number (required, alphanumeric 6 chars)
  - `destinationBankAccountNo` — beneficiary account number (required, alphanumeric up to 12 chars)
  - `destinationBankAccountName` — beneficiary account name/title (required, string up to 32 chars)
- A Direct Entry batch submitted without these fields will fail validation with a clear error message
- **This is a breaking change to the Direct Entry request contract** for any existing caller; migration required before upgrade

### Modules/Files Modified
- `src/CommBiz.Api/Features/DirectEntry/`
  - `ConvertDirectEntryBatchRequest.cs` — Added `DestinationBankBsb`, `DestinationBankAccountNo`, `DestinationBankAccountName` fields (required)
  - `DirectEntryDetailRecordMapper.cs` — Maps Detail Record primary fields from new Destination* request fields (not Trace settings)
  - `DirectEntrySelfBalancingRecordMapper.cs` — Rewritten to use dedicated `SelfBalancingAccountNo`, `SelfBalancingNameOfRemitter`, `SelfBalancingLodgementReferenceDetails` settings with hardcoded transaction code `"13"` and Title constant
  - `DirectEntryHeaderRecordMapper.cs` — Header record's `InstitutionCode`, `UserIdentificationNumber`, `NameOfUserSupplyingFile` now hardcoded mapper constants (removed from settings)
  - `DirectEntrySettings.cs` — Removed `InstitutionCode`, `UserIdentificationNumber`, `NameOfUserSupplyingFile`, `Title`, `TransactionCode`; added `SelfBalancingAccountNo`, `SelfBalancingNameOfRemitter`, `SelfBalancingLodgementReferenceDetails`
  - `DirectEntryValidator.cs` — Added validation for new Destination* fields (identical to Source* field validation rules)

- `src/CommBiz.Api/Features/PaymentRouting/`
  - `ConvertToFileRouter.cs` [new] — New router for `/convert-to-file` endpoint, reuses `PaymentTypeRouter` dispatch logic
  - `PaymentTypeRouter.cs` — `GetPaymentTypeCode` method made `internal` for reuse by `ConvertToFileRouter`

- `src/CommBiz.Api/Program.cs` [modified]
  - Registered new `POST /convert-to-file` route (temporary, marked with TODO comment for permanence decision)

- `src/CommBiz.Api/appsettings.json`, `appsettings.Development.json` [modified]
  - DirectEntry configuration updated to include new `SelfBalancingAccountNo`, `SelfBalancingNameOfRemitter`, `SelfBalancingLodgementReferenceDetails` settings
  - Removed obsolete settings (`InstitutionCode`, `UserIdentificationNumber`, `NameOfUserSupplyingFile`, `Title`, `TransactionCode` in config)

- `tests/CommBiz.Api.Tests/DirectEntry/` [modified]
  - Unit tests updated to reflect new Destination* request fields and field-mapping corrections
  - Validator tests expanded to cover new Destination* field validation
  - Mapper tests updated to verify Detail Record pulls from Destination* fields (not Trace), Self-Balancing uses dedicated settings

- `tests/CommBiz.Api.Tests/PaymentRouting/` [modified]
  - New integration tests for `POST /convert-to-file` endpoint (all payment types)

- `tests/smoke/` [reorganised]
  - `Errors.http` [new] — Consolidated cross-cutting rejection scenarios (e.g., missing Destination* fields, malformed requests)
  - `DirectEntry.http` [modified] — Updated to include new Destination* fields; smoke tests refactored to use new field-mapping
  - `PriorityPayments.http` [modified] — Reorganised to align with new Errors.http structure

### Breaking Changes
**Direct Entry Request Contract Changes** — `PaymentInstructionRequest` for `"DE"`-typed payment instructions now requires:
- `destinationBankBsb` (new, required)
- `destinationBankAccountNo` (new, required)
- `destinationBankAccountName` (new, required)

Any caller of `/convert` or `/convert-to-file` with `"DE"` payment type **must** supply these three fields or requests will fail validation. This is a **breaking change**; existing integrations submitting Direct Entry batches must be updated before upgrading to 1.6.0.

### Test Coverage
**385 passing tests** (verified via `dotnet test`, 0 failed, 0 skipped; 0 vulnerabilities via `dotnet list package --vulnerable --include-transitive`):

1. **Happy Path — Direct Entry Destination Account Mapping Correctness**
   - POST valid Direct Entry batch with new Destination* fields (`destinationBankBsb`, `destinationBankAccountNo`, `destinationBankAccountName`) to `POST /convert` with payment type `"DE"`
   - Verify Detail Record's primary BSB/Account Number/Title are populated from the Destination* fields (the beneficiary account, not remitter)
   - Confirm Trace section populated from organisation's `TraceAccountBsb`/`TraceAccountAccNo`/`NameOfRemitter` (unchanged)
   - Validate output against real CommBank test account credentials to confirm payment now credits the intended destination
   - Confirm `ConvertedText` formatted correctly and matches expected Direct Entry spec

2. **Edge Case — Missing Destination Fields Validation Rejection**
   - POST Direct Entry batch omitting any one of `destinationBankBsb`, `destinationBankAccountNo`, or `destinationBankAccountName`
   - Verify validation rejects batch with clear error indicating which Destination* field is missing
   - Confirm response structure matches existing error handling (validation error envelope)
   - POST batch with invalid Destination field formats (e.g., BSB wrong length, account number exceeds max width)
   - Verify validation catches format violations identically to Source* field validation

3. **Regression-Sensitive — Self-Balancing Record Reconciliation**
   - POST valid Direct Entry batch with new Destination* fields and self-balancing record enabled
   - Verify Self-Balancing record uses dedicated `SelfBalancingAccountNo`, `SelfBalancingNameOfRemitter`, `SelfBalancingLodgementReferenceDetails` settings (not Detail Record settings)
   - Confirm Self-Balancing record has hardcoded transaction code `"13"` (debit) and hardcoded Title constant
   - Validate trailer reconciles to zero net (Detail records' total credits, Self-Balancing record's total debit, sum to zero)
   - POST valid Revision 6 payloads (F-001–F-024) with all payment types (DE, BPAY, IMT, RTGS, FOREX)
   - Verify output byte-for-byte identical to Revision 6 behavior (no regression for non-DE payment types)
   - Confirm `/convert` endpoint (JSON response) remains unchanged; new `/convert-to-file` endpoint correctly returns file download

## [1.5.0] — 2026-08-18

### Added

#### FX Conversion Pipeline
- **F-023:** New FX batch conversion pipeline for foreign exchange (FOREX) payment routing
  - Converts FX instruction batches to IPFX CSV format (CBA-compliant international payment file specification)
  - Routed on payment type `"FOREX"` via `PaymentTypeRouter`
  - Validator enforces FX-specific batch constraints (max 200 instructions, max 15 unique currency pairs)
  - Instruction-level validation for required fields, format, and currency code compliance
  - `FxRecordMapper` converts batch metadata and instructions to IPFX-compliant field mapping
  - Handler orchestrates validation, mapping, and conversion into final formatted output
  - Unified `Mappings` model (F-021) included for field-level traceability across FX conversion

- **F-022:** Payment Type Router extended to dispatch `"FOREX"` payment type code
  - Added `"FOREX"` case to `PaymentTypeRouter` routing switch
  - Dispatches `"FOREX"` to `ConvertFxBatchCommand`
  - No changes to existing routing logic (DE, BPAY, IMT, RTGS dispatch unchanged)

### Modules/Files Modified
- `src/CommBiz.Api/Features/Fx/` [new]
  - `FxPaymentInstructionRequest.cs` — Request model for individual FX instructions
  - `FxSettings.cs` — Configuration and settings for FX processing (field widths, constraints)
  - `FxValidator.cs` — Batch-level validation (instruction count ≤ 200, currency pairs ≤ 15)
  - `FxRecordMapper.cs` — Maps FX instructions to IPFX CSV format with field mapping metadata
  - `ConvertFxBatchCommand.cs` — MediatR command for orchestration
  - `ConvertFxBatchHandler.cs` — Handler implementing the FX conversion pipeline
  - `ConvertFxBatchResponse.cs` — Response model with `ConvertedText` and `Mappings`

- `src/CommBiz.Api/Features/PaymentRouting/PaymentTypeRouter.cs` [modified]
  - Added `"FOREX"` case to payment type routing switch
  - Dispatches `"FOREX"` to `ConvertFxBatchCommand`
  - No changes to existing routing logic (DE, BPAY, IMT, RTGS dispatch unchanged)

- `src/CommBiz.Api/Program.cs` [modified]
  - Registered `FxSettings` in dependency injection container
  - MediatR handler auto-discovery includes FX batch handler
  - No changes to existing service registrations

- `src/CommBiz.Api/appsettings.json` [modified]
  - Added `"Fx"` configuration section with IPFX field definitions and validation constraints
  - Currency pair and instruction limit configuration for batch boundaries

### Breaking Changes
None — FX conversion is purely additive. Existing payment types (Direct Entry, BPAY, IMT, Priority Payments) are unaffected. New payment type routed on `"FOREX"` string; no changes to existing request/response contracts or routing logic.

### Test Coverage
**374 passing tests** (verified via `dotnet test`, 0 failed, 0 skipped; 0 vulnerabilities via `dotnet list package --vulnerable --include-transitive`):

1. **Happy Path — Valid FX Batch Conversion to IPFX CSV**
   - POST valid FX batch with multiple instructions (≤ 200 instructions, ≤ 15 unique currency pairs) to `POST /convert` with payment type `"FOREX"`
   - Verify response includes `ConvertedText` (formatted IPFX CSV output) and populated `Mappings` array
   - Confirm `Mappings` is ordered parallel to `ConvertedText` with per-instruction field breakdown
   - Validate output meets IPFX/CBA-spec CSV format constraints (field widths, delimiters, currency code format)

2. **Edge Case — Batch Boundary Enforcement**
   - POST FX batch at exactly 200 instructions, 15 unique currency pairs (maximum allowed)
   - Verify response includes `ConvertedText` and `Mappings` with all 200 instructions converted
   - POST FX batch exceeding 200 instructions or 15 currency pairs
   - Verify validation rejects batch with clear error indicating boundary violation
   - Confirm response structure matches existing error handling (validation error case)

3. **Regression-Sensitive — Existing Payment Type Routing Unaffected**
   - POST valid Direct Entry batch to `POST /convert` with payment type `"DE"`
   - Verify response unchanged from Revision 5 (conversion output identical, Mappings present)
   - POST valid BPAY batch with payment type `"BPAY"`, verify output unchanged
   - POST valid IMT batch with payment type `"FOREX_DOMESTIC"` (if applicable) or test IMT, verify unaffected
   - POST valid Priority Payments batch with payment type `"RTGS"`, verify output unchanged
   - Confirm no regression: existing handlers for all four payment types still route correctly and produce byte-for-byte identical output

## [1.4.0] — 2026-08-18

### Added

#### Priority Payments Conversion Pipeline
- **F-018:** New Priority Payments batch conversion pipeline for RTGS (Real-Time Gross Settlement) payment routing
  - Shares International Money Transfers (IMT) file format with domestic-only field restrictions
  - Routed on payment type `"RTGS"` via `PaymentTypeRouter`
  - Validator enforces Priority Payments-specific batch constraints and instruction rules
  - `PriorityPaymentsSettingsMapper` converts batch metadata to Priority Payments request/response structure
  - Handler orchestrates validation, mapping, and conversion into final formatted output
  - Unified `Mappings` model (F-021) included from day one for field-level traceability

#### Phase 2 Complete: Additional Payment Types
- All five Phase 2 features now shipped and integrated (F-015, F-016, F-017, F-018, F-021)
  - F-015: Payment Type Router dispatcher (unified routing surface)
  - F-016: BPAY batch payments conversion
  - F-017: International Money Transfers (IMT) CSV format
  - F-018: Priority Payments RTGS routing (this release)
  - F-021: Shared Field Mapping Model (integrated into all payment types)
  - Direct Entry foundation (Phase 1) remains unchanged and unaffected

### Modules/Files Modified
- `src/CommBiz.Api/Features/PriorityPayments/` [new]
  - `ConvertPriorityPaymentsBatchRequest.cs` — Request model for Priority Payments batch input
  - `PriorityPaymentsSettings.cs` — Configuration and settings for Priority Payments processing
  - `PriorityPaymentsBatchValidator.cs` — Batch-level validation (instruction count, field presence)
  - `PriorityPaymentsInstructionValidator.cs` — Per-instruction validation and constraint enforcement
  - `PriorityPaymentsBatchMapper.cs` — Maps batch instructions to CBA-spec format
  - `ConvertPriorityPaymentsBatchCommand.cs` — MediatR command for orchestration
  - `ConvertPriorityPaymentsBatchHandler.cs` — Handler implementing the conversion pipeline
  - `ConvertPriorityPaymentsBatchResponse.cs` — Response model with `ConvertedText` and `Mappings`

- `src/CommBiz.Api/PaymentTypeRouter.cs` [modified]
  - Added `"RTGS"` case to payment type routing switch
  - Dispatches `"RTGS"` to `ConvertPriorityPaymentsBatchCommand`
  - No changes to existing routing logic (DE, BPAY, IMT dispatch unchanged)

- `src/CommBiz.Api/Program.cs` [modified]
  - Registered `PriorityPaymentsSettings` in dependency injection container
  - MediatR handler auto-discovery includes Priority Payments handler
  - No changes to existing service registrations

- `src/CommBiz.Api/appsettings.json` [modified]
  - Added `"PriorityPayments"` configuration section (mirrors IMT structure for shared file format)
  - Field definitions and validation constraints for RTGS payment routing

### Breaking Changes
None — Priority Payments is purely additive. Existing payment types (Direct Entry, BPAY, IMT) are unaffected. New payment type routed on `"RTGS"` string; no changes to existing request/response contracts or routing logic.

### Test Coverage
**318 passing tests** (verified via `dotnet test`, 0 failed, 0 skipped; 0 vulnerabilities via `dotnet list package --vulnerable --include-transitive`):

1. **Happy Path — Valid Priority Payments Batch Conversion**
   - POST valid Priority Payments batch with multiple instructions to `POST /convert` with payment type `"RTGS"`
   - Verify response includes `ConvertedText` (formatted RTGS output) and populated `Mappings` array
   - Confirm `Mappings` is ordered parallel to `ConvertedText` with per-instruction field breakdown
   - Validate output meets RTGS/IMT-derived file format constraints (domestic-only fields only)

2. **Edge Case — Mixed Payment Type Rejection**
   - POST batch mixing `"RTGS"` payment type with another payment type (e.g., `"BPAY"` instruction in same batch)
   - Verify validation rejects the batch with clear error indicating mixed payment types not allowed
   - Confirm response structure matches existing error handling (validation error case)

3. **Regression-Sensitive — Invalid Instruction Handling**
   - POST Priority Payments batch with invalid instruction code (e.g., malformed or missing)
   - Verify response has `ConvertedText: null` and `Mappings: null` with appropriate error
   - POST same invalid scenario to existing payment types (DE, BPAY, IMT)
   - Confirm no regression: existing handlers still return `Mappings: null` on validation failure
   - POST valid Revision 3 payloads (F-001–F-020) to all payment types
   - Verify output byte-for-byte identical to Revision 4 behavior (Phase 2 additions don't affect Phase 1)

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
