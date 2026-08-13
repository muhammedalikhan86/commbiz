# Graph Report - src  (2026-08-13)

## Corpus Check
- Corpus is ~1,488 words - fits in a single context window. You may not need a graph.

## Summary
- 84 nodes · 104 edges · 8 communities (7 shown, 1 thin omitted)
- Extraction: 89% EXTRACTED · 11% INFERRED · 0% AMBIGUOUS · INFERRED: 11 edges (avg confidence: 0.83)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- Validation & Rules
- Community 1
- Routing & Dispatch
- Validation & Rules
- Validation & Rules
- Application Setup
- Community 6
- Design Patterns

## God Nodes (most connected - your core abstractions)
1. `DirectEntryValidator` - 10 edges
2. `CommBiz.Api.Features.DirectEntry` - 9 edges
3. `ConvertDirectEntryBatchCommand` - 8 edges
4. `http` - 6 edges
5. `https` - 6 edges
6. `PaymentInstructionRequest` - 5 edges
7. `ConvertDirectEntryBatchRequest` - 4 edges
8. `Fixed-Width File Assembly` - 4 edges
9. `PaymentInstructionError` - 3 edges
10. `DirectEntryDetailRecordMapper` - 3 edges

## Surprising Connections (you probably didn't know these)
- `Direct Entry Validator` --shares_data_with--> `Fixed-Width File Assembly`  [INFERRED]
  src/CommBiz.Api/Features/DirectEntry/DirectEntryValidator.cs → src/CommBiz.Api/Features/DirectEntry/README.md
- `Detail Record Mapper` --participates_in--> `Fixed-Width File Assembly`  [INFERRED]
  src/CommBiz.Api/Features/DirectEntry/DirectEntryDetailRecordMapper.cs → src/CommBiz.Api/Features/DirectEntry/README.md
- `Header Record Mapper` --participates_in--> `Fixed-Width File Assembly`  [INFERRED]
  src/CommBiz.Api/Features/DirectEntry/DirectEntryHeaderRecordMapper.cs → src/CommBiz.Api/Features/DirectEntry/README.md
- `Trailer Record Mapper` --participates_in--> `Fixed-Width File Assembly`  [INFERRED]
  src/CommBiz.Api/Features/DirectEntry/DirectEntryTrailerRecordMapper.cs → src/CommBiz.Api/Features/DirectEntry/README.md
- `ConvertDirectEntryBatchCommand` --calls--> `Payment Type Router`  [INFERRED]
  src/CommBiz.Api/Features/DirectEntry/ConvertDirectEntryBatchCommand.cs → src/CommBiz.Api/Features/DirectEntry/PaymentTypeRouter.cs

## Import Cycles
- None detected.

## Hyperedges (group relationships)
- **Direct Entry Processing Pipeline** — batch_request_model, payment_type_router, direct_entry_validator, detail_record_mapper, header_record_mapper, trailer_record_mapper, fixed_width_assembly, batch_response_model [INFERRED 0.85]
- **Vertical Slice Implementation** — direct_entry_slice, vertical_slice_arch, convert_direct_entry_command, batch_request_model, batch_response_model [EXTRACTED 1.00]

## Communities (8 total, 1 thin omitted)

### Community 0 - "Validation & Rules"
Cohesion: 0.13
Nodes (10): CommBiz.Api.Features.DirectEntry, ConvertDirectEntryBatchRequest, PaymentInstructionRequest, DirectEntryDetailRecordMapper, string, DirectEntryHeaderRecordMapper, string, DirectEntryTrailerRecordMapper (+2 more)

### Community 1 - "Community 1"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 2 - "Routing & Dispatch"
Cohesion: 0.20
Nodes (7): ConvertDirectEntryBatchCommand, ConvertDirectEntryBatchHandler, ConvertDirectEntryBatchResponse, PaymentInstructionError, PaymentTypeRouter, IReadOnlyList, string

### Community 3 - "Validation & Rules"
Cohesion: 0.26
Nodes (6): DirectEntryValidator, IReadOnlyList, string, IEnumerable, int, long

### Community 4 - "Validation & Rules"
Cohesion: 0.24
Nodes (11): ConvertDirectEntryBatchRequest, ConvertDirectEntryBatchResponse, ConvertDirectEntryBatchCommand, Detail Record Mapper, Direct Entry Validator, Fixed-Width File Assembly, Header Record Mapper, Kestrel Minimal API Host (+3 more)

### Community 5 - "Application Setup"
Cohesion: 0.32
Nodes (5): CommBiz.Api.Features.Diagnostics, PingCommand, PingHandler, PingResult, Program

### Community 6 - "Community 6"
Cohesion: 0.40
Nodes (4): net10.0, WolverineFx (6.26.0), WolverineFx.RuntimeCompilation (6.26.0), Microsoft.NET.Sdk.Web

## Knowledge Gaps
- **18 isolated node(s):** `net10.0`, `WolverineFx (6.26.0)`, `WolverineFx.RuntimeCompilation (6.26.0)`, `Microsoft.NET.Sdk.Web`, `Program` (+13 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **1 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `CommBiz.Api.Features.DirectEntry` connect `Validation & Rules` to `Routing & Dispatch`, `Application Setup`?**
  _High betweenness centrality (0.175) - this node is a cross-community bridge._
- **Why does `DirectEntryValidator` connect `Validation & Rules` to `Validation & Rules`?**
  _High betweenness centrality (0.077) - this node is a cross-community bridge._
- **Why does `PaymentInstructionRequest` connect `Validation & Rules` to `Routing & Dispatch`, `Validation & Rules`?**
  _High betweenness centrality (0.066) - this node is a cross-community bridge._
- **Are the 6 inferred relationships involving `ConvertDirectEntryBatchCommand` (e.g. with `Detail Record Mapper` and `Direct Entry Validator`) actually correct?**
  _`ConvertDirectEntryBatchCommand` has 6 INFERRED edges - model-reasoned connections that need verification._
- **What connects `net10.0`, `WolverineFx (6.26.0)`, `WolverineFx.RuntimeCompilation (6.26.0)` to the rest of the system?**
  _18 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Validation & Rules` be split into smaller, more focused modules?**
  _Cohesion score 0.13071895424836602 - nodes in this community are weakly interconnected._
- **Should `Community 1` be split into smaller, more focused modules?**
  _Cohesion score 0.13333333333333333 - nodes in this community are weakly interconnected._