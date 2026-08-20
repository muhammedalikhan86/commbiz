# Revision History

**Current Revision:** 7
## Revision 7
- **Date:** 2026-08-20
- **Description:** Direct Entry field-mapping correctness fix and new `/convert-to-file` file-download endpoint
- **Features:** Amended F-003/F-005/F-006/F-014 (field-mapping bug fix); new temporary file-download endpoint (F-025 informal)
- **Test Coverage:** 385 passing tests (verified via `dotnet test`); 0 vulnerabilities

## Revision 6
- **Date:** 2026-08-18
- **Description:** FX conversion pipeline for FOREX payment type; fifth payment type added
- **Features:** F-022, F-023 (Payment Type Router FOREX extension, FX batch conversion pipeline)
- **Test Coverage:** 374 passing tests (verified via `dotnet test`); 0 vulnerabilities

## Revision 5
- **Date:** 2026-08-18
- **Description:** Priority Payments conversion pipeline; Phase 2 (Additional Payment Types) complete
- **Features:** F-018; Phase 2 closure (F-015, F-016, F-017, F-018, F-021 all Done)
- **Test Coverage:** 318 passing tests (verified via `dotnet test`); 0 vulnerabilities

## Revision 4
- **Date:** 2026-08-17
- **Description:** Shared Field Mapping Model with per-line mapping metadata for all payment types
- **Features:** F-021
- **Test Coverage:** 244 passing tests (verified via `dotnet test`)
## Revision 3
- **Date:** 2026-08-17
- **Description:** Payment Type Router extended to real cross-slice dispatcher; BPAY and IMT batch conversion support
- **Features:** F-015, F-016, F-017
- **Test Coverage:** 206 passing tests (verified via `dotnet test`)
## Revision 2
- **Date:** 2026-08-14
- **Description:** Self-balancing record support and minimum batch size reduction
- **Features:** F-014
- **Test Coverage:** 79 passing tests (verified via `dotnet test`)
## Revision 1
- **Date:** 2026-08-13
- **Description:** Initial release — Direct Entry batch conversion pipeline
- **Features:** F-001, F-002, F-003, F-004, F-005, F-006, F-007, F-008
- **Test Coverage:** 106 passing tests
