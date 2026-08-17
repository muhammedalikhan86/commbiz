# Graph Report - src  (2026-08-17)

## Corpus Check
- Corpus is ~6,402 words - fits in a single context window. You may not need a graph.

## Summary
- 208 nodes · 367 edges · 11 communities (10 shown, 1 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 16 edges (avg confidence: 0.82)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- Community 0
- Community 1
- Community 2
- Community 3
- Community 4
- Community 5
- Community 6
- Community 7
- Community 8
- Community 9
- Community 10

## God Nodes (most connected - your core abstractions)
1. `PaymentInstructionRequest` - 15 edges
2. `CommBiz.Api.Features.Shared` - 15 edges
3. `DirectEntrySettings` - 12 edges
4. `ImtValidator` - 12 edges
5. `CommBiz.Api.Features.DirectEntry` - 11 edges
6. `PaymentTypeRouter` - 11 edges
7. `ImtRecordMapper` - 10 edges
8. `CommBiz.Api.Features.BPay` - 8 edges
9. `BPayPaymentInstructionRequest` - 8 edges
10. `FieldMapping` - 8 edges

## Surprising Connections (you probably didn't know these)
- `Shared Field Mapping Model` --shared_data_with--> `BPAY Vertical Slice`  [INFERRED]
  src/CommBiz.Api/Features/Shared/FieldMapping.cs → src/CommBiz.Api/Features/BPay/README.md
- `Shared Field Mapping Model` --shared_data_with--> `Direct Entry Vertical Slice`  [INFERRED]
  src/CommBiz.Api/Features/Shared/FieldMapping.cs → src/CommBiz.Api/Features/DirectEntry/README.md
- `Shared Field Mapping Model` --shared_data_with--> `IMT Vertical Slice`  [INFERRED]
  src/CommBiz.Api/Features/Shared/FieldMapping.cs → src/CommBiz.Api/Features/Imt/README.md
- `Payment Type Router` --dispatches_to--> `BPAY Vertical Slice`  [EXTRACTED]
  src/CommBiz.Api/Features/PaymentRouting/README.md → src/CommBiz.Api/Features/BPay/README.md
- `Direct Entry Vertical Slice` --references--> `ADR-002: Vertical Slice Architecture`  [EXTRACTED]
  src/CommBiz.Api/Features/DirectEntry/README.md → src/CommBiz.Api/Features/BPay/README.md

## Import Cycles
- None detected.

## Communities (11 total, 1 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.11
Nodes (24): AmountTotalField, AmountTotalInCents, DateToBeProcessed, DescriptionOfEntriesOnFile, PaymentInstructionRequest, IReadOnlyList, DirectEntryDetailRecordMapper, IReadOnlyList (+16 more)

### Community 1 - "Community 1"
Cohesion: 0.09
Nodes (22): CommBiz.Api.Features.BPay, BPayDetailRecordMapper, IReadOnlyList, string, BPayHeaderRecordMapper, DateTime, IReadOnlyList, string (+14 more)

### Community 2 - "Community 2"
Cohesion: 0.13
Nodes (12): CommBiz.Api.Features.Imt, ConvertImtBatchCommand, ConvertImtBatchHandler, ConvertImtBatchResponse, PaymentInstructionError, ImtPaymentInstructionRequest, ImtRecordMapper, IReadOnlyList (+4 more)

### Community 3 - "Community 3"
Cohesion: 0.11
Nodes (12): CommBiz.Api.Features.Shared, CommBiz.Api.Features.DirectEntry, ConvertDirectEntryBatchCommand, ConvertDirectEntryBatchHandler, ConvertDirectEntryBatchResponse, PaymentInstructionError, DirectEntryAmountTotals, DirectEntryHeaderRecordMapper (+4 more)

### Community 4 - "Community 4"
Cohesion: 0.18
Nodes (12): CommBiz.Api.Features.PaymentRouting, PaymentRoutingError, PaymentRoutingResponse, PaymentTypeRouter, int, string, IMessageBus, IResult (+4 more)

### Community 5 - "Community 5"
Cohesion: 0.12
Nodes (17): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, launchUrl, applicationUrl (+9 more)

### Community 6 - "Community 6"
Cohesion: 0.21
Nodes (6): ImtValidator, DateTime, decimal, IEnumerable, int, IReadOnlyList

### Community 7 - "Community 7"
Cohesion: 0.27
Nodes (5): DirectEntryValidator, decimal, IEnumerable, int, IReadOnlyList

### Community 8 - "Community 8"
Cohesion: 0.48
Nodes (7): ADR-002: Vertical Slice Architecture, BPAY Vertical Slice, Direct Entry Vertical Slice, F-021: Shared Field Mapping Retrofit, IMT Vertical Slice, Payment Type Router, Shared Field Mapping Model

### Community 9 - "Community 9"
Cohesion: 0.29
Nodes (6): net10.0, Microsoft.AspNetCore.OpenApi (10.0.11), Scalar.AspNetCore (2.16.20), WolverineFx (6.26.0), WolverineFx.RuntimeCompilation (6.26.0), Microsoft.NET.Sdk.Web

## Knowledge Gaps
- **21 isolated node(s):** `net10.0`, `Microsoft.AspNetCore.OpenApi (10.0.11)`, `Scalar.AspNetCore (2.16.20)`, `WolverineFx (6.26.0)`, `WolverineFx.RuntimeCompilation (6.26.0)` (+16 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **1 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `FieldMapping` connect `Community 0` to `Community 1`, `Community 2`, `Community 3`?**
  _High betweenness centrality (0.173) - this node is a cross-community bridge._
- **Why does `CommBiz.Api.Features.Shared` connect `Community 3` to `Community 1`, `Community 2`?**
  _High betweenness centrality (0.121) - this node is a cross-community bridge._
- **What connects `net10.0`, `Microsoft.AspNetCore.OpenApi (10.0.11)`, `Scalar.AspNetCore (2.16.20)` to the rest of the system?**
  _21 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Community 0` be split into smaller, more focused modules?**
  _Cohesion score 0.10512820512820513 - nodes in this community are weakly interconnected._
- **Should `Community 1` be split into smaller, more focused modules?**
  _Cohesion score 0.08819345661450925 - nodes in this community are weakly interconnected._
- **Should `Community 2` be split into smaller, more focused modules?**
  _Cohesion score 0.12535612535612536 - nodes in this community are weakly interconnected._
- **Should `Community 3` be split into smaller, more focused modules?**
  _Cohesion score 0.1067193675889328 - nodes in this community are weakly interconnected._