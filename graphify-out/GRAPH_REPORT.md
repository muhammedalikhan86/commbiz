# Graph Report - src  (2026-08-18)

## Corpus Check
- Corpus is ~8,361 words - fits in a single context window. You may not need a graph.

## Summary
- 253 nodes · 440 edges · 18 communities (8 shown, 10 thin omitted)
- Extraction: 97% EXTRACTED · 3% INFERRED · 0% AMBIGUOUS · INFERRED: 12 edges (avg confidence: 0.8)
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
- Community 11
- Community 12
- Community 13
- Community 14
- Community 15
- Community 16
- Community 17

## God Nodes (most connected - your core abstractions)
1. `CommBiz.Api.Features.Shared` - 18 edges
2. `PaymentInstructionRequest` - 15 edges
3. `DirectEntrySettings` - 12 edges
4. `PaymentTypeRouter` - 12 edges
5. `ImtValidator` - 12 edges
6. `PriorityPaymentValidator` - 12 edges
7. `CommBiz.Api.Features.DirectEntry` - 11 edges
8. `ImtRecordMapper` - 10 edges
9. `FieldMapping` - 9 edges
10. `BPayPaymentInstructionRequest` - 8 edges

## Surprising Connections (you probably didn't know these)
- None detected - all connections are within the same source files.

## Import Cycles
- None detected.

## Communities (18 total, 10 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.10
Nodes (28): AmountTotalField, AmountTotalInCents, DateToBeProcessed, DescriptionOfEntriesOnFile, PaymentInstructionRequest, IReadOnlyList, DirectEntryDetailRecordMapper, IReadOnlyList (+20 more)

### Community 1 - "Community 1"
Cohesion: 0.08
Nodes (20): CommBiz.Api.Features.PriorityPayments, ConvertPriorityPaymentBatchCommand, ConvertPriorityPaymentBatchHandler, ConvertPriorityPaymentBatchResponse, PaymentInstructionError, PriorityPaymentInstructionRequest, PriorityPaymentRecordMapper, GeneratedRegex (+12 more)

### Community 2 - "Community 2"
Cohesion: 0.08
Nodes (18): CommBiz.Api.Features.Imt, ConvertImtBatchCommand, ConvertImtBatchHandler, ConvertImtBatchResponse, PaymentInstructionError, ImtPaymentInstructionRequest, ImtRecordMapper, GeneratedRegex (+10 more)

### Community 3 - "Community 3"
Cohesion: 0.09
Nodes (22): CommBiz.Api.Features.BPay, BPayDetailRecordMapper, IReadOnlyList, string, BPayHeaderRecordMapper, DateTime, IReadOnlyList, string (+14 more)

### Community 4 - "Community 4"
Cohesion: 0.09
Nodes (13): CommBiz.Api.Features.Shared, CommBiz.Api.Features.DirectEntry, ConvertDirectEntryBatchCommand, ConvertDirectEntryBatchHandler, ConvertDirectEntryBatchResponse, PaymentInstructionError, DirectEntryAmountTotals, DirectEntryValidator (+5 more)

### Community 5 - "Community 5"
Cohesion: 0.19
Nodes (12): CommBiz.Api.Features.PaymentRouting, PaymentRoutingError, PaymentRoutingResponse, PaymentTypeRouter, int, string, IMessageBus, IResult (+4 more)

### Community 6 - "Community 6"
Cohesion: 0.12
Nodes (17): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, launchUrl, applicationUrl (+9 more)

### Community 7 - "Community 7"
Cohesion: 0.29
Nodes (6): net10.0, Microsoft.AspNetCore.OpenApi (10.0.11), Scalar.AspNetCore (2.16.20), WolverineFx (6.26.0), WolverineFx.RuntimeCompilation (6.26.0), Microsoft.NET.Sdk.Web

## Knowledge Gaps
- **22 isolated node(s):** `LineMapping`, `PaymentRoutingError`, `Program`, `applicationUrl`, `commandName` (+17 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **10 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.