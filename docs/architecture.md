# Architecture: Shaw and Partners → CommBank Payment File Conversion Service

> Status: APPROVED
> Version: v4
> Last updated: 2026-08-13
> PRD: docs/prd.md (built against v6)

## 1. System Context
Shaw and Partners' internal systems call this service with a group of payment instructions to
convert. The service converts them into CommBank's Direct Entry file format (the first
supported payment type; further types are added in later tranches — see PRD Goals). The
service does not submit anything to CommBank itself, does not call any other external system,
and does not persist any data — it is a stateless request/response conversion step sitting
between Shaw and Partners' systems and whatever process ultimately hands the output to CommBiz.

## 2. High-Level Architecture
A single ASP.NET Core Minimal API service (.NET 10), self-hosted directly on Kestrel with no
containerization. The codebase is organised as vertical slices — one slice per payment type
(starting with Direct Entry), each slice owning its own request model, validation, mapping, and
output-assembly logic end to end, rather than sharing horizontal layers (e.g. no shared
generic "mapping layer" or "repository layer"). Wolverine is used as the in-process
command/handler pipeline for each slice (CQRS), replacing the need for a separate mediator
library. No database is used — the service holds no state beyond a single request's lifetime.

## 3. Components

### API Host
- **Responsibility:** Exposes the HTTP Minimal API endpoint(s) that accept a batch conversion
  request and return a result.
- **Inputs:** HTTP request containing an array of payment instructions.
- **Outputs:** JSON response containing either the converted Direct Entry content as text, or
  a validation failure with a reason per invalid instruction.
- **Dependencies:** Wolverine pipeline for dispatching the request to the correct slice.
- **Technology:** ASP.NET Core Minimal API, .NET 10, Kestrel.

### Payment Type Router
- **Responsibility:** Inspects each instruction's declared payment type and dispatches the
  batch to the corresponding slice. Rejects the batch if it contains a payment type not yet
  supported (only Direct Entry in this tranche).
- **Inputs:** Parsed batch of payment instructions.
- **Outputs:** Dispatch to a specific slice's Wolverine command, or a rejection.
- **Dependencies:** Direct Entry Conversion Slice (today); future slices as added.
- **Technology:** Wolverine command routing.

### Direct Entry Conversion Slice
- **Responsibility:** Validates and converts a batch of Direct-Entry-typed instructions into a
  CommBank Direct Entry file: one header record, one detail record per instruction, one trailer
  record, per the fixed-width layout in the Direct Entry spec.
- **Inputs:** Validated batch of Direct-Entry-typed payment instructions.
- **Outputs:** Assembled Direct Entry file content (header + details + trailer, CRLF-terminated
  120-character records).
- **Dependencies:** Validation component.
- **Technology:** Plain C# mapping code (no AutoMapper/commercial mapping library, per
  Technology Decisions).

### Validation Component
- **Responsibility:** Validates every instruction in the batch against Direct Entry field
  rules (e.g. BSB format, account number rules, amount format, mandatory fields) before any
  conversion happens. If any instruction fails, the entire batch is rejected with a reason per
  invalid instruction (per PRD Q5) — no partial conversion.
- **Inputs:** Raw batch of payment instructions.
- **Outputs:** Either "all valid" (proceed to conversion) or a list of per-instruction
  validation failures.
- **Dependencies:** None (pure validation logic).
- **Technology:** Plain C# validation, run as part of the Wolverine pipeline for the slice.

## 4. Data Flow

Primary use case — converting a batch of Direct Entry payment instructions:

1. Shaw and Partners' system sends a batch of payment instructions to the API Host.
2. The Payment Type Router checks every instruction's payment type. If any instruction's type
   isn't supported yet, the whole batch is rejected.
3. The Direct Entry Conversion Slice's Validation Component checks every instruction against
   Direct Entry field rules.
4. If any instruction is invalid, the batch is rejected in full, with a validation reason
   returned for every invalid instruction. No output is produced.
5. If all instructions are valid, each is mapped to a Direct Entry detail record; a header
   record is built from batch-level metadata; a trailer record is built with computed totals
   (credit total, debit total, net total, record count) so the file is self-balancing.
6. The assembled file content is returned to the caller directly, as text within the JSON
   response body, alongside a success indicator — no download link, no temporary storage.

## 5. Functional Requirements

| ID | Requirement | Source (PRD story) | Priority |
|----|-------------|-------------------|----------|
| FR-001 | Accept a batch (group) of payment instructions in a single request | "support converting a group of payment instructions submitted together" | Must |
| FR-002 | Validate every instruction against Direct Entry field rules before conversion | "notified when a payment run cannot be converted" | Must |
| FR-003 | Reject the entire batch, with a reason per invalid instruction, if any instruction is invalid | Q5 — whole-group rejection | Must |
| FR-004 | Convert every valid Direct-Entry-typed instruction into a compliant header/detail/trailer Direct Entry file | Core conversion goal | Must |
| FR-005 | Compute self-balancing trailer totals (net/credit/debit amounts, detail record count) | Direct Entry spec requirement | Must |
| FR-006 | Reject a batch containing a payment type not yet supported, rather than silently dropping or mis-converting it | Phased payment-type rollout goal | Must |

## 6. Non-Functional Requirements

| ID | Category | Requirement | Notes |
|----|----------|-------------|-------|
| NFR-001 | Performance | Convert a batch within an acceptable latency for on-demand, synchronous use | Target figure is `[TBD]` — deferred, see Open Questions & Risks, A4 |
| NFR-002 | Security | No payment instruction data (account numbers, amounts, names) is persisted or written to logs in plaintext | Service is stateless by design (no database); logging must still be deliberately scoped |
| NFR-003 | Security | No application-level authentication required | Service is deployed in a trusted internal-only network; access control is a network/deployment concern, not an app concern |
| NFR-004 | Reliability | Service holds no state between requests, so any instance can serve any request | Enabled directly by the no-database constraint; no in-memory cache is used either |
| NFR-005 | Scalability | Service can run multiple instances behind a load balancer without coordination | Consequence of NFR-004; no longer at risk since output is returned inline, not via a cached link |

## 7. Technology Decisions

| Concern | Choice | ADR | Rationale |
|---------|--------|-----|-----------|
| Runtime & API hosting | .NET 10, ASP.NET Core Minimal API | ADR-001 | Directed choice; minimal API fits a small, focused conversion endpoint without controller/MVC ceremony |
| Code organisation | Vertical slice architecture (one slice per payment type) | ADR-002 | Directed choice; keeps each payment type's conversion logic self-contained as more types are added |
| CQRS / in-process dispatch | Wolverine | ADR-003 | Directed choice; provides command/handler pipeline without a general-purpose mediator library |
| Object mapping | Manual mapping (no AutoMapper or other commercial mapping library) | ADR-004 | Directed choice; avoids a commercial/general-purpose dependency for a small, fixed mapping |
| Hosting/deployment | Self-hosted directly on Kestrel; no containerization, no database | ADR-005 | Directed choice; service is stateless and simple enough not to need a database or container runtime |
| Logging/observability | Shaw.Diagnostics (in-house NuGet, from `\\sic2tfs1\nuget\Packages`), version 2.0.0 | ADR-006 | Directed choice; standardises on the organisation's existing diagnostics package rather than a general-purpose logging library |
| Response format for converted output | Return the assembled Direct Entry content inline as text within a JSON response body | ADR-008 | Directed choice; simpler than a download link and avoids any server-side result caching (see rejected ADR-007) |

## 8. Open Questions & Risks

| ID | Question / Risk | Impact | Owner | Resolved? |
|----|----------------|--------|-------|-----------|
| A1 | Should the converted Direct Entry output be returned directly in the HTTP response, or written to a location for later pickup (e.g. for CommBiz submission)? | High — shapes the API Host's response contract and the Data Flow's final step | User | Yes — returned directly, as text within a JSON response body. No download link, no result caching (ADR-007 rejected in favour of ADR-008). |
| A2 | Does the service need application-level authentication (e.g. API key, mTLS), or is it deployed in a trusted internal-only network? | High — security posture (NFR-003) | User | Yes — no application-level auth; internal-only deployment. |
| A3 | Can a single submitted batch mix payment types (with unsupported types causing rejection), or must every instruction in a batch declare the same, currently-supported type? | Medium — affects Payment Type Router behaviour (FR-006) | User | Yes — reject the batch if it contains any instruction of an unsupported/not-yet-usable type. Noted as a decision the user may revisit in a later tranche. |
| A4 | What is the expected/maximum batch size and target conversion latency? | Medium — ties to PRD's open turnaround-time metric and NFR-001 | User | Deferred — explicitly revisited later. |
| A5 | What are the logging/traceability requirements for audit purposes (per PRD's compliance/audit user story), given no data is persisted? | Medium — affects NFR-002 and how "traceable back to source" is satisfied without a database | User | Yes — logging library decided (Shaw.Diagnostics, ADR-006); detailed logging design left for implementation. |

## Version History
| Version | Date | Change | Triggered By |
|---------|------|--------|---------------|
| v1 | 2026-08-13 | Initial draft | — |
| v2 | 2026-08-13 | Resolved A2 (no auth, internal-only), A3 (reject batch on unsupported type, revisitable), A5 (Shaw.Diagnostics logging, ADR-006); deferred A4; proposed a Conversion Result Store + download-link design for A1 (ADR-007, PROPOSED) pending confirmation | Triage edit |
| v3 | 2026-08-13 | Reversed A1: no download link after all — converted output returned inline as text in a JSON response. Removed Conversion Result Store component; rejected ADR-007; added ADR-008 (inline JSON response) | Triage edit |
| v4 | 2026-08-13 | Architecture approved (A4 remains open, deferred by user request) | Gate approval |
