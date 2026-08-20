# Architecture: Shaw and Partners → CommBank Payment File Conversion Service

> Status: APPROVED
> Version: v13
> Last updated: 2026-08-20
> PRD: docs/prd.md (built against v9)

## 1. System Context
Shaw and Partners' internal systems call this service with a group of payment instructions to
convert. The service converts them into CommBank's Direct Entry file format (the first
supported payment type; further types are added in later tranches — see PRD Goals). The
service does not submit anything to CommBank itself, does not call any other external system,
and does not persist any data — it is a stateless request/response conversion step sitting
between Shaw and Partners' systems and whatever process ultimately hands the output to CommBiz.

## 2. High-Level Architecture
A single ASP.NET Core Minimal API service (.NET 10), self-hosted directly on Kestrel with no
containerization. A single generic HTTP endpoint (`POST /convert`) accepts a batch of payment
instructions in Shaw and Partners' own native payload shape; routing to the correct payment-type
slice happens internally, keyed on each instruction's declared payment type code — the endpoint
itself is payment-type-agnostic, not path-per-type. The codebase is organised as vertical slices —
one slice per payment type (starting with Direct Entry), each slice owning its own request model,
validation, mapping, and output-assembly logic end to end, rather than sharing horizontal layers
(e.g. no shared generic "mapping layer" or "repository layer"). Wolverine is used as the
in-process command/handler pipeline for each slice (CQRS), replacing the need for a separate
mediator library. No database is used — the service holds no state beyond a single request's
lifetime. Organisation-level constants each payment type's output format requires (e.g. Direct
Entry's remitter details, trace account) are sourced from static application configuration, not
carried in the request; a small number of values that never vary at all regardless of
environment (e.g. Direct Entry's institution code, transaction codes) are hardcoded mapper
constants instead of configuration — the upstream payload only ever contains the data that
genuinely varies per instruction, which includes the destination/beneficiary bank details Direct
Entry credits (see §3, Direct Entry Configuration).

## 3. Components

### API Host
- **Responsibility:** Exposes a single generic HTTP Minimal API endpoint (`POST /convert`) that
  accepts a batch conversion request and returns a result. The endpoint is payment-type-agnostic;
  it does not know which slice will ultimately handle the batch.
- **Inputs:** HTTP request body containing a plain JSON array of payment instructions, in Shaw
  and Partners' own native payload shape (e.g. `paymentTypeCode`, `accountNo`,
  `sourceBankAccountName`, `sourceBankAccountNo`, `sourceBankBSB`, `destinationBankAccountName`,
  `destinationBankAccountNo`, `destinationBankBsb`, `paymentDate`, `amount`, `createBy`) — not a
  shape pre-mapped to any single bank format's fields.
- **Outputs:** JSON response containing either the converted output content as text plus its
  parallel field-by-field mapping breakdown (see Field Mapping Model below), or a validation
  failure with a reason per invalid instruction.
- **Dependencies:** Wolverine pipeline for dispatching the request to the correct slice.
- **Technology:** ASP.NET Core Minimal API, .NET 10, Kestrel.

> **Note (experimental, not an approved architectural component):** `POST /convert-to-file`
> (`Features/PaymentRouting/ConvertToFileRouter.cs`) exists in the codebase alongside `POST
> /convert`, reusing the same Payment Type Router dispatch across all five payment types. On a
> successful conversion it returns `ConvertedText` as a downloadable `.txt` file (`Results.File`)
> instead of inline JSON, falling back to the standard JSON error envelope on rejection/failure.
> It is explicitly commented `// TEMPORARY` in source by its author and has not been through this
> project's Developer/Reviewer/Integration loop. It is not a reintroduction of the rejected
> ADR-007 design — no server-side caching, persistence, separate `GET /conversions/{id}/file`
> lookup, or expiry — and does not change `/convert`'s own ADR-008 inline-JSON behaviour. Tracked
> as PMBook F-024/PM-013 pending a product decision to keep, formalize, or remove it.

### Payment Type Router
- **Responsibility:** The single top-level, cross-slice dispatcher — explicitly not a vertical
  slice itself (ADR-002 exception: this is the one genuinely shared, cross-slice component,
  since routing has to see across all payment types before any single slice can own the
  request). Peeks each instruction's `paymentTypeCode` on the raw JSON batch and rejects the
  whole batch outright if: the batch is empty; instructions declare more than one distinct
  payment type (a mixed batch); or every instruction shares a single type that isn't wired to a
  slice yet. Once a single, recognised type is confirmed for the whole batch, deserializes the
  raw JSON into that slice's own request shape and dispatches to that slice's own Wolverine
  command, untouched — each slice still owns its own request/response/validator/mapper end to
  end; this component only owns the dispatch decision.
- **Inputs:** Parsed batch of payment instructions (raw JSON array).
- **Outputs:** Dispatch to the corresponding slice's Wolverine command (Direct Entry, BPay,
  IMT, Priority Payments, or FX), or a rejection — batch-level reason for empty/mixed batches,
  per-instruction reason for an unsupported type.
- **Dependencies:** Direct Entry Conversion Slice; BPay Conversion Slice; IMT Conversion Slice;
  Priority Payments Conversion Slice; FX Conversion Slice.
- **Technology:** Wolverine command routing; `System.Text.Json` for the `paymentTypeCode` peek
  and per-slice deserialization.

### Direct Entry Conversion Slice
- **Responsibility:** Validates and converts a batch of Direct-Entry-typed instructions into a
  CommBank Direct Entry file: one header record, one detail record per instruction, one
  self-balancing (contra) detail record, then one trailer record, per the fixed-width layout in
  the Direct Entry spec. Each detail record's primary destination fields — BSB Number, Account
  Number to be Credited, Title of Account to be Credited — are sourced from the request's
  `DestinationBank*` fields (the real beneficiary/destination account being credited), not from
  configuration; corrected in v13/PMBook v28 from the original implementation, which sourced
  these primary fields from organisation-level Trace settings instead (a real bug — Direct Entry
  payments were not crediting the intended destination account — now fixed and confirmed against
  real CommBank test accounts). Fields the upstream payload does not carry (because they are
  constant for every Direct Entry submission Shaw and Partners makes — e.g. lodgement reference,
  trace BSB/account, name of remitter, withholding tax) are populated from the Direct Entry
  Configuration component; a small number of values that never vary at all (institution code,
  name of user supplying file, transaction code) are hardcoded mapper constants instead of
  configuration. The self-balancing detail record posts the batch's total amount against a
  dedicated settlement account (`SelfBalancingAccountNo`, distinct from the regular detail
  records' Trace BSB/Account) with a dedicated Name of Remitter and Lodgement Reference Details
  (`SelfBalancingNameOfRemitter`/`SelfBalancingLodgementReferenceDetails`), all from Direct Entry
  Configuration; its Title and Transaction Code are hardcoded mapper constants, not configuration,
  so the file's credit and debit totals reconcile (per the Direct Entry spec's
  self-balancing/contra-entry requirement, docs/stash/Direct Entry - File
  Specification CommBiz.md §1).
- **Inputs:** Validated batch of Direct-Entry-typed payment instructions; Direct Entry
  Configuration.
- **Outputs:** Assembled Direct Entry file content (header + details + self-balancing detail
  record + trailer, CRLF-terminated 120-character records), plus its parallel `LineMapping`
  breakdown (header/detail(s)/selfbalancing/trailer — see Field Mapping Model).
- **Dependencies:** Validation component; Direct Entry Configuration.
- **Technology:** Plain C# mapping code (no AutoMapper/commercial mapping library, per
  Technology Decisions).

### BPay Conversion Slice
- **Responsibility:** Validates and converts a batch of BPAY-typed instructions into a CommBank
  BPAY Batch Payments CSV file: one header record plus one Payment Details record (`50,`) per
  instruction, comma-delimited and CRLF-terminated. Unlike Direct Entry, there is no trailer or
  self-balancing record — the file is simply Header + Details, per the BPay spec. Fields the
  upstream payload does not carry (funding account, file number) are sourced from BPay
  Configuration, not the request.
- **Inputs:** Validated batch of BPAY-typed payment instructions; BPay Configuration.
- **Outputs:** Assembled BPAY CSV file content (header record + one Payment Details record per
  instruction, CRLF-terminated), plus its parallel `LineMapping` breakdown (header/detail(s) —
  see Field Mapping Model).
- **Dependencies:** BPayValidator — this slice's own validation logic, not shared with Direct
  Entry or IMT; BPay Configuration (`BPaySettings`: funding account, file number).
- **Technology:** Plain C# mapping code (`BPayHeaderRecordMapper` / `BPayDetailRecordMapper`),
  no AutoMapper/commercial mapping library, per Technology Decisions.

### IMT Conversion Slice
- **Responsibility:** Validates and converts a batch of IMT-typed instructions (routed on Shaw
  and Partners' internal `TT`/"Telegraphic Transfer" code; every output row still writes the
  literal constant `IMT` per the CBA file spec) into a CommBank IMT/MT101-family file: one
  27-field CSV row per instruction, CRLF-separated between rows, with no trailing CRLF after the
  last row. Unlike Direct Entry and BPay, there is no header or trailer record at all. Country
  code fields are derived from characters 5-6 of the relevant SWIFT BIC rather than a discrete
  payload field; the debit account number field is sourced entirely from IMT Configuration,
  never the request. Legal/identity fields (account number/name, bank names, SWIFT codes) are
  rejected outright on invalid characters or overlong values; free-text fields (beneficiary
  address, payment details) are sanitized (disallowed characters replaced with a space) instead
  of rejected.
- **Inputs:** Validated batch of IMT-typed payment instructions; IMT Configuration.
- **Outputs:** Assembled IMT file content (one 27-field CSV row per instruction, CRLF-separated,
  no trailing CRLF, no header/trailer record), plus its parallel `LineMapping` breakdown (one
  `row1`/`row2`... entry per instruction — see Field Mapping Model).
- **Dependencies:** ImtValidator — this slice's own validation logic, not shared with Direct
  Entry or BPay; IMT Configuration (`ImtSettings`: debit account BSB/number/name).
- **Technology:** Plain C# mapping code (`ImtRecordMapper`), no AutoMapper/commercial mapping
  library, per Technology Decisions.

### Priority Payments Conversion Slice
- **Responsibility:** Validates and converts a batch of Priority-Payments-typed instructions
  (routed on Shaw and Partners' internal `RTGS` code — also known as Priority Payments; every
  output row still writes the literal constant `PP` per the CBA file spec, never the API's own
  routing code) into a CommBank Priority Payments/MT101-family file: one 27-field CSV row per
  instruction, CRLF-separated between rows, with no trailing CRLF after the last row. Shares the
  same MT101 file family as the IMT Conversion Slice but enforces its own, stricter field rules
  per the MT101 spec's §1.5 (a 14-month process-date window rather than IMT's 7-day window; an
  exactly-6-digit, unpadded beneficiary bank BSB rather than Direct Entry's `nnn-nnn`; stricter
  beneficiary name/address character rules than IMT's, disallowing hyphens/apostrophes). The
  debit account number field is sourced entirely from Priority Payments Configuration, never the
  request's own source bank fields (which are carried but unused, the same treatment IMT gives
  its own unused fields). Built directly against the shared Field Mapping Model from its first
  commit, not retrofitted.
- **Inputs:** Validated batch of Priority-Payments-typed payment instructions; Priority Payments
  Configuration.
- **Outputs:** Assembled Priority Payments file content (one 27-field CSV row per instruction,
  CRLF-separated, no trailing CRLF, no header/trailer record), plus its parallel `LineMapping`
  breakdown (one `row1`/`row2`... entry per instruction — see Field Mapping Model).
- **Dependencies:** PriorityPaymentValidator — this slice's own validation logic, not shared with
  Direct Entry, BPay, or IMT; Priority Payments Configuration (`PriorityPaymentsSettings`: debit
  account BSB/number/name).
- **Technology:** Plain C# mapping code (`PriorityPaymentRecordMapper`), no AutoMapper/commercial
  mapping library, per Technology Decisions.

### FX Conversion Slice
- **Responsibility:** Validates and converts a batch of FX-typed instructions (routed on Shaw
  and Partners' `FOREX` code) into a CommBank CommBiz IPFX Bulk Settlement Upload file: one CSV
  data row per instruction, comma-delimited, CRLF-terminated, with no header or footer row (per
  the IPFX spec's own business rules) and no self-balancing record. Each row always writes the
  literal constant `FX` as Transaction Type. The I SELL/I BUY Instruction fields (7/12) and the
  I BUY/I SELL Payment details fields (21/22) always come from static FX Configuration, not the
  request, since Shaw and Partners' FX requests are never routed to an actual settlement account
  or a different settlement type (the IPFX spec's Samples 1/3/4/5 "Address Book Beneficiary"
  pattern) — every row uses the spec's Sample 2 shape (I SELL Instruction = `MAN`, I BUY
  Instruction = `DOC`). The beneficiary/intermediary bank fields (8/9/10/11/13/14/20), however,
  are still mapped through from the request whenever present, using the same shared payload
  fields (`IntermediaryBankSwiftCode`, `DestinationBankSwiftCode`, `DestinationBankAccountName`,
  `BeneficiaryAddress`) the IMT Conversion Slice already maps for its own beneficiary/
  intermediary bank fields — country codes (9/11/20) are derived from the SWIFT BIC's characters
  5-6, same technique as IMT. This is pass-through only (truncated to each field's max length,
  no character-set or reject-on-invalid validation yet). Beneficiary Address lines 2/3 (15/16)
  stay blank regardless — a hard spec rule, not a data gap. Beneficiary City/State/Postcode
  (17-19) stay blank too — no discrete field for them exists anywhere in the shared payload,
  same gap the IMT Conversion Slice documents. Transaction Description is sourced from the
  request's `accountNo` field (max 12AN). The request's single `Amount` value is always placed
  on the Sell side (I SELL Amount), leaving I BUY Amount blank, mirroring the IPFX spec's Sample 2
  shape. Fields specific to IDR/CNH/KRW currency pairs (Purpose of Payment, CNAPS Code,
  Beneficiary Company Name/Contact/SSN) are not yet supported (see Open Questions & Risks, A6) —
  unlike 8-20, no source data for these exists in the payload at all.
  `PaymentSourceTypeCode`, `PaymentDate`, `Notes`, `RateTypeCode`, `ValueDateTypeCode`,
  `FeeTypeCode`, and `FeeOtherTypeCode` are carried in the request but unused — none of the IPFX
  file's 27 field positions correspond to them.
- **Inputs:** Validated batch of FX-typed payment instructions; FX Configuration.
- **Outputs:** Assembled FX CSV file content (one data row per instruction, CRLF-terminated, no
  header/trailer), plus its parallel `LineMapping` breakdown (one `row1`/`row2`... entry per
  instruction — see Field Mapping Model).
- **Dependencies:** FxValidator — this slice's own validation logic, not shared with the other
  slices; FX Configuration (`FxSettings`: I SELL/I BUY Instruction codes, I BUY/I SELL Payment
  details static text).
- **Technology:** Plain C# mapping code (`FxRecordMapper`), no AutoMapper/commercial mapping
  library, per Technology Decisions.

### FX Configuration
- **Responsibility:** Holds the organisation-level constants an FX file requires that never vary
  per instruction — I SELL Instruction (`MAN`), I BUY Instruction (`DOC`), I BUY Payment details
  (`Buy`), I SELL Payment details (`Sell`). Kept out of the request payload and out of source
  code as hardcoded defaults, adjustable via configuration without a code change, same pattern as
  Direct Entry/BPay/IMT/Priority Payments Configuration.
- **Inputs:** None (static configuration, not request-driven).
- **Outputs:** Confirmed static values consumed by the FX Conversion Slice's row mapping.
- **Dependencies:** None.
- **Technology:** ASP.NET Core `IOptions<T>` configuration binding, sourced from
  `appsettings.json`.

### Field Mapping Model
- **Responsibility:** A shared, cross-slice type representing a field-by-field breakdown of a
  converted line — the second explicit exception to ADR-002's no-shared-layer rule (alongside
  the Payment Type Router), justified because the shape is identical across every conversion
  slice and has no per-type variation. Every conversion slice's mapper is extended to build this
  breakdown alongside (not instead of) its existing raw text/CSV output, from the same field
  values it already maps. Each line of the converted output (header, one entry per detail
  record, self-balancing record, trailer — whichever apply to that payment type) gets one entry
  in an ordered list, keyed by record type and, for repeating records, occurrence (e.g.
  `header`, `detail1`, `detail2`, `selfbalancing`, `trailer` for Direct Entry) — never a raw
  positional line number, so the key stays meaningful independent of file position. Each field
  within a line's entry records both sides of the mapping: the request-side origin (the request
  object's field name or the static appsettings field name, plus the value that was used) and
  the CBA response side (the field name per the relevant CommBank spec, plus whatever value was
  actually placed in the output — which may be null/empty). A line's field list covers every
  spec-defined field position for that record type, including reserved/blank/unused positions,
  not only the ones a given instruction happens to populate — so a line's `Fields` entries stay
  in 1:1 positional correspondence with that line's own `ConvertedText` output.
- **Inputs:** The same per-instruction request fields and static configuration values each
  slice's existing mapper already consumes.
- **Outputs:** An ordered list of line entries (`LineMapping`), each carrying an ordered list of
  field entries (`FieldMapping`: request field, request value, CBA response field, CBA response
  value) — returned as `Mappings` in the response, parallel to `ConvertedText`. A List, not a
  Dictionary, is used at both levels so line order (matching the assembled file's own order) and
  field order (matching the spec's field order) are preserved deterministically — JSON object/
  dictionary key order is not a reliable guarantee.
- **Dependencies:** None (pure data-shape type consumed by every Conversion Slice's mapper).
- **Technology:** Plain C# records (`FieldMapping`, `LineMapping`), no AutoMapper/commercial
  mapping library, per Technology Decisions and ADR-009.

### Validation Component
- **Responsibility:** Validates every instruction in the batch against the field rules that
  apply to the data the upstream payload actually carries (e.g. source bank BSB/account format,
  amount bounds, mandatory identifiers) before any conversion happens. If any instruction fails,
  the entire batch is rejected with a reason per invalid instruction (per PRD Q5) — no partial
  conversion. Also enforces a minimum of 1 payment instruction: the Direct Entry spec's
  minimum-2-detail-record structural rule is satisfied by the Conversion Slice's own
  self-balancing detail record (every batch of 1+ valid instructions yields 2+ detail records in
  the output), so the batch itself no longer needs to supply 2 instructions.
- **Inputs:** Raw batch of payment instructions.
- **Outputs:** Either "all valid" (proceed to conversion) or a list of per-instruction
  validation failures.
- **Dependencies:** None (pure validation logic).
- **Technology:** Plain C# validation, run as part of the Wolverine pipeline for the slice.

### Direct Entry Configuration
- **Responsibility:** Holds the organisation-level constants a Direct Entry file requires that
  vary only by environment/configuration, not by instruction or request — description of entries,
  lodgement reference, trace BSB/account, name of remitter, withholding tax amount, plus the
  self-balancing record's dedicated settlement account, name of remitter, and lodgement reference
  (`SelfBalancingAccountNo`/`SelfBalancingNameOfRemitter`/`SelfBalancingLodgementReferenceDetails`).
  Values that never vary at all regardless of environment — CommBank institution code, Shaw and
  Partners' name/title/APCA user identification number, transaction codes — are hardcoded mapper
  constants instead, kept out of both the request payload and configuration.
- **Inputs:** None (static configuration, not request-driven).
- **Outputs:** Confirmed static values consumed by the Direct Entry Conversion Slice's Header,
  Detail, and Trailer record mapping.
- **Dependencies:** None.
- **Technology:** ASP.NET Core `IOptions<T>` configuration binding, sourced from `appsettings.json`.

## 4. Data Flow

Routing now fans out to five slices — Direct Entry (`DE`), BPay (`BPAY`), IMT (`TT`), Priority
Payments (`RTGS`), and FX (`FOREX`) — all dispatched through the same Payment Type Router. Direct
Entry remains the primary worked example below, since it is the most structurally involved
(header + details + self-balancing record + trailer); see the note after step 6 for how BPay,
IMT, Priority Payments, and FX differ.

1. Shaw and Partners' system sends a batch of payment instructions, in its own native payload
   shape, to the API Host's `POST /convert` endpoint.
2. The Payment Type Router checks every instruction's declared payment type code, rejecting the
   batch outright if it's empty, mixes payment types, or if every instruction shares a single
   type that isn't wired to a slice yet. If the batch passes, it is dispatched to that type's own
   conversion slice.
3. The dispatched slice's own Validation Component checks every instruction against the field
   rules that apply to the data the payload carries, plus the batch-level minimum-1-instruction
   structural rule (Direct Entry's output's own self-balancing detail record accounts for the
   Direct Entry spec's minimum-2-detail-record rule).
4. If any instruction is invalid, the batch is rejected in full, with a validation reason
   returned for every invalid instruction. No output is produced.
5. If all instructions are valid, each is mapped to a Direct Entry detail record — the
   destination BSB/account/title fields come from the request's `DestinationBank*` fields, and
   the remaining fields from Direct Entry Configuration or hardcoded mapper constants; a header
   record is built from Direct Entry Configuration and hardcoded mapper constants plus the
   earliest instruction's payment date; a self-balancing (contra) detail record is built from the
   batch's total amount and Direct Entry Configuration's dedicated self-balancing settlement
   account/remitter/lodgement-reference details, with a hardcoded Transaction Code; a trailer
   record is built with computed totals (credit total, debit total, net total, record count — all
   including the self-balancing detail record) so the file is self-balancing.
6. The assembled file content is returned to the caller directly, as text within the JSON
   response body, alongside a success indicator — no download link, no temporary storage.
   Alongside it, the same mapping step that populated each record's fields also emits that
   record's `LineMapping` entry (see Field Mapping Model), so the response carries an ordered,
   parallel `Mappings` list — one entry per line of the converted output — for the testing team
   to verify the output field-by-field without parsing the raw fixed-width/CSV text.

**BPay, IMT, Priority Payments, and FX follow the same overall shape** (validate → map →
assemble → return inline) but with slice-specific field rules and output formats: BPay assembles
a CSV Header + one Payment Details record per instruction, with no trailer or self-balancing
record; IMT and Priority Payments each assemble one 27-field CSV row per instruction,
CRLF-separated, with no header or trailer record and no trailing CRLF after the last row,
sharing the same MT101 file family but enforcing their own distinct field rules per the MT101
spec; FX assembles one CommBiz IPFX data row per instruction, CRLF-separated, with no header or
footer row, its settlement-instruction and payment-details fields sourced from static FX
Configuration rather than the request. See the BPay Conversion Slice, IMT Conversion Slice,
Priority Payments Conversion Slice, and FX Conversion Slice entries in §3 for their full
field/output rules.

## 5. Functional Requirements

| ID | Requirement | Source (PRD story) | Priority |
|----|-------------|-------------------|----------|
| FR-001 | Accept a batch (group) of payment instructions in a single request | "support converting a group of payment instructions submitted together" | Must |
| FR-002 | Validate every instruction against Direct Entry field rules before conversion | "notified when a payment run cannot be converted" | Must |
| FR-003 | Reject the entire batch, with a reason per invalid instruction, if any instruction is invalid | Q5 — whole-group rejection | Must |
| FR-004 | Convert every valid Direct-Entry-typed instruction into a compliant header/detail/trailer Direct Entry file | Core conversion goal | Must |
| FR-005 | Compute self-balancing trailer totals (net/credit/debit amounts, detail record count), including the self-balancing detail record introduced by FR-007 | Direct Entry spec requirement | Must |
| FR-006 | Reject a batch containing a payment type not yet supported, rather than silently dropping or mis-converting it | Phased payment-type rollout goal | Must |
| FR-007 | Generate a self-balancing (contra) detail record — posting the batch's total amount against the configured settlement account, in the direction opposite the batch's Transaction Code — immediately before the trailer record, on every conversion | Direct Entry spec's self-balancing/contra-entry requirement; PRD v7 | Must |
| FR-008 | Accept a batch of as few as 1 valid instruction — the minimum-2-detail-record structural rule is satisfied by FR-007's self-balancing detail record, not by the batch's own size | PRD v7 (single-instruction runs are a fully supported batch size) | Must |
| FR-009 | Every successful conversion response includes an ordered, parallel `Mappings` list — one entry per line of the converted output (header/detail/self-balancing/trailer as applicable), each with a request-field/value and CBA-response-field/value breakdown per field — regardless of payment type | PRD v8 (testing-team convenience goal) | Must |
| FR-010 | Convert every valid FX-typed instruction into a CommBiz IPFX Bulk Settlement CSV row, per the confirmed field mapping (constant Transaction Type; `accountNo` as Transaction Description; Buy/Sell Currency and Amount; static Instruction and Payment details fields from FX Configuration) | PRD v9 (FX conversion goal) | Must |

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
| Field-level mapping breakdown | Shared `FieldMapping`/`LineMapping` records, one shared type reused by every conversion slice, returned as an ordered `Mappings` list parallel to `ConvertedText` | ADR-009 | Identical shape needed across all payment types with no per-type variation; a second sanctioned ADR-002 exception, alongside the Payment Type Router |

## 8. Open Questions & Risks

| ID | Question / Risk | Impact | Owner | Resolved? |
|----|----------------|--------|-------|-----------|
| A1 | Should the converted Direct Entry output be returned directly in the HTTP response, or written to a location for later pickup (e.g. for CommBiz submission)? | High — shapes the API Host's response contract and the Data Flow's final step | User | Yes — returned directly, as text within a JSON response body. No download link, no result caching (ADR-007 rejected in favour of ADR-008). |
| A2 | Does the service need application-level authentication (e.g. API key, mTLS), or is it deployed in a trusted internal-only network? | High — security posture (NFR-003) | User | Yes — no application-level auth; internal-only deployment. |
| A3 | Can a single submitted batch mix payment types (with unsupported types causing rejection), or must every instruction in a batch declare the same, currently-supported type? | Medium — affects Payment Type Router behaviour (FR-006) | User | Yes — reject the batch if it contains any instruction of an unsupported/not-yet-usable type. Noted as a decision the user may revisit in a later tranche. |
| A4 | What is the expected/maximum batch size and target conversion latency? | Medium — ties to PRD's open turnaround-time metric and NFR-001 | User | Deferred — explicitly revisited later. |
| A5 | What are the logging/traceability requirements for audit purposes (per PRD's compliance/audit user story), given no data is persisted? | Medium — affects NFR-002 and how "traceable back to source" is satisfied without a database | User | Yes — logging library decided (Shaw.Diagnostics, ADR-006); detailed logging design left for implementation. |
| A6 | The FX Conversion Slice currently only supports the IPFX spec's non-CBA "Instruction" pattern (`MAN`/`DOC`) for USD/AUD-style currency pairs. If a future FX instruction uses IDR, CNH, or KRW as the Buy currency, the spec requires additional conditional fields (Purpose of Payment, CNAPS Code, Beneficiary Company Name/Contact/SSN) that aren't yet mapped. Should these be built now, or deferred until an actual IDR/CNH/KRW requirement arises? | Low — no current requirement for these currencies | User | No — deferred, not blocking initial FX delivery. |

## Version History
| Version | Date | Change | Triggered By |
|---------|------|--------|---------------|
| v1 | 2026-08-13 | Initial draft | — |
| v2 | 2026-08-13 | Resolved A2 (no auth, internal-only), A3 (reject batch on unsupported type, revisitable), A5 (Shaw.Diagnostics logging, ADR-006); deferred A4; proposed a Conversion Result Store + download-link design for A1 (ADR-007, PROPOSED) pending confirmation | Triage edit |
| v3 | 2026-08-13 | Reversed A1: no download link after all — converted output returned inline as text in a JSON response. Removed Conversion Result Store component; rejected ADR-007; added ADR-008 (inline JSON response) | Triage edit |
| v4 | 2026-08-13 | Architecture approved (A4 remains open, deferred by user request) | Gate approval |
| v5 | 2026-08-13 | Reshaped the API Host/Payment Type Router/Direct Entry Conversion Slice/Validation Component descriptions to match the real upstream payload contract (`POST /convert`, payment-type-code-driven routing rather than a per-type URL path); added a new Direct Entry Configuration component documenting that organisation-level constants (institution code, remitter, trace account, transaction code, etc.) are sourced from application configuration rather than the request; updated Data Flow accordingly | Implementation-driven correction |
| v6 | 2026-08-13 | Added the self-balancing (contra) detail record requirement (FR-007): the Direct Entry Conversion Slice now emits a config-derived contra detail record, posted against the settlement (Trace BSB/Account) account in the direction opposite the batch's Transaction Code, immediately before the trailer; reduced the batch-level minimum from 2 to 1 instruction (FR-008), since this new record itself satisfies the spec's minimum-2-detail-record rule; updated Validation Component, Data Flow steps 3/5, and FR-005 accordingly | User requirement change |
| v7 | 2026-08-17 | Documentation catch-up for F-015/F-016/F-017 (Reviewer-PASS'd since 2026-08-14) — architecture.md had not been updated since v6/Phase 1. Replaced the Payment Type Router entry (was DE-only) with its real cross-slice dispatcher behaviour (empty/mixed/unsupported rejection, ADR-002 exception); added BPay Conversion Slice and IMT Conversion Slice component entries, each with its own slice-owned validator; updated Data Flow to reflect fan-out to DE/BPAY/IMT with a note on BPay/IMT's differing output shapes (no trailer; IMT also has no header and no trailing CRLF); confirmed FR-006 and §7 Technology Decisions still read correctly for the now-multi-type reality, no changes needed | Documentation catch-up |
| v8 | 2026-08-17 | Added FR-009 and a new shared Field Mapping Model component (`FieldMapping`/`LineMapping` records) exposing a per-line, per-field breakdown of every conversion response (request field/value + CBA response field/value), parallel to `ConvertedText`; a second sanctioned ADR-002 exception (added ADR-009); updated API Host outputs and Data Flow step 6 accordingly. Applies to DE, BPay, and IMT (retrofit — all already Done) and to the not-yet-built F-018 (Priority Payments), tracked as new PMBook item F-021 | Triage edit — user requirement change; upward ripple to PRD v8 |
| v9 | 2026-08-17 | Clarified the Field Mapping Model description to state explicitly that a line's field list covers every spec-defined field position for that record type (including reserved/blank/unused positions), not only populated ones — the wording previously left this ambiguous, which is what let the original F-021 implementation silently skip unpopulated positions until PMBook v17's correction fixed the code | Integration Agent Documentation Drift finding on the F-021 correction |
| v10 | 2026-08-17 | Added the missing Priority Payments Conversion Slice component entry to §3 (F-018, Done, had no architecture.md entry) and updated §4 Data Flow's fan-out note to include Priority Payments (`RTGS`) alongside DE/BPAY/IMT | Integration Agent Documentation Drift finding on F-018 |
| v11 | 2026-08-18 | Added FX as a fifth payment type: new FX Conversion Slice and FX Configuration components (§3) per the confirmed CommBiz IPFX Bulk Settlement Upload field mapping (constant Transaction Type; `accountNo` → Transaction Description; Buy/Sell Currency/Amount; static Instruction and Payment details fields; IDR/CNH/KRW-specific fields deferred, see A6); updated Payment Type Router's dispatch list (also fixed a pre-existing gap where Priority Payments was missing from it); updated §4 Data Flow fan-out note; added FR-010; added Open Question A6 | Ripple from PRD v9 |
| v12 | 2026-08-18 | Architecture approved | Gate approval |
| v13 | 2026-08-20 | Corrected stale Direct Entry field-source claims after the user's direct-commit bug fix (PMBook v28): institution code, name of user supplying file, and transaction code (detail `"50"`, self-balancing `"13"`) are hardcoded mapper constants, not config, and were wrongly described as config-sourced in §2/§3; detail records' BSB/Account/Title to be Credited now come from the request's `DestinationBank*` fields, not Direct Entry Configuration (§3 Direct Entry Conversion Slice, previously wrong); self-balancing record now posts against a dedicated `SelfBalancingAccountNo`/`SelfBalancingNameOfRemitter`/`SelfBalancingLodgementReferenceDetails`, not the regular detail records' Trace BSB/Account, and there is no more "batch's configured Transaction Code" to post opposite (§3, §4 step 5, previously wrong on both counts); updated the Direct Entry Configuration component's held-settings list accordingly; added an experimental note on `POST /convert-to-file` (API Host, not a finished component) | Integration Agent Documentation Drift finding on the user's direct-commit round; Finalizer Architecture correction pass |
