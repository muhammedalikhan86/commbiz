# Graph Report - src  (2026-08-20)

## Corpus Check
- Corpus is ~10,080 words - fits in a single context window. You may not need a graph.

## Summary
- 302 nodes · 505 edges · 32 communities (12 shown, 20 thin omitted)
- Extraction: 98% EXTRACTED · 2% INFERRED · 0% AMBIGUOUS · INFERRED: 12 edges (avg confidence: 0.8)
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
- Community 16
- Community 18
- Community 19
- Community 20
- Community 21
- Community 22
- Community 23
- Community 24
- Community 25
- Community 26
- Community 27
- Community 28
- Community 29
- Community 30
- Community 31

## God Nodes (most connected - your core abstractions)
1. `CommBiz.Api.Features.Shared` - 21 edges
2. `PaymentInstructionRequest` - 14 edges
3. `PaymentTypeRouter` - 13 edges
4. `ImtValidator` - 12 edges
5. `PriorityPaymentValidator` - 12 edges
6. `DirectEntrySettings` - 11 edges
7. `CommBiz.Api.Features.DirectEntry` - 11 edges
8. `ImtRecordMapper` - 10 edges
9. `FieldMapping` - 10 edges
10. `BPayPaymentInstructionRequest` - 8 edges

## Surprising Connections (you probably didn't know these)
- None detected - all connections are within the same source files.

## Import Cycles
- None detected.

## Communities (32 total, 20 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.10
Nodes (25): AmountTotalField, AmountTotalInCents, DateToBeProcessed, DescriptionOfEntriesOnFile, IReadOnlyList, PaymentInstructionRequest, IReadOnlyList, DirectEntryDetailRecordMapper (+17 more)

### Community 1 - "Community 1"
Cohesion: 0.08
Nodes (20): CommBiz.Api.Features.PriorityPayments, ConvertPriorityPaymentBatchCommand, ConvertPriorityPaymentBatchHandler, ConvertPriorityPaymentBatchResponse, PaymentInstructionError, PriorityPaymentInstructionRequest, PriorityPaymentRecordMapper, GeneratedRegex (+12 more)

### Community 2 - "Community 2"
Cohesion: 0.08
Nodes (18): CommBiz.Api.Features.Imt, ConvertImtBatchCommand, ConvertImtBatchHandler, ConvertImtBatchResponse, PaymentInstructionError, ImtPaymentInstructionRequest, ImtRecordMapper, GeneratedRegex (+10 more)

### Community 3 - "Community 3"
Cohesion: 0.09
Nodes (21): CommBiz.Api.Features.BPay, BPayDetailRecordMapper, string, BPayHeaderRecordMapper, DateTime, IReadOnlyList, string, BPayPaymentInstructionRequest (+13 more)

### Community 4 - "Community 4"
Cohesion: 0.13
Nodes (18): ConvertedText, CommBiz.Api.Features.PaymentRouting, ConvertToFileRouter, IMessageBus, IResult, JsonElement, Task, PaymentRoutingError (+10 more)

### Community 5 - "Community 5"
Cohesion: 0.09
Nodes (18): CommBiz.Api.Features.Fx, ConvertFxBatchCommand, ConvertFxBatchHandler, ConvertFxBatchResponse, PaymentInstructionError, FxPaymentInstructionRequest, FxRecordMapper, IReadOnlyList (+10 more)

### Community 6 - "Community 6"
Cohesion: 0.13
Nodes (8): CommBiz.Api.Features.Shared, CommBiz.Api.Features.DirectEntry, ConvertDirectEntryBatchCommand, ConvertDirectEntryBatchHandler, ConvertDirectEntryBatchResponse, PaymentInstructionError, DirectEntryAmountTotals, LineMapping

### Community 7 - "Community 7"
Cohesion: 0.12
Nodes (17): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, launchUrl, applicationUrl (+9 more)

### Community 8 - "Community 8"
Cohesion: 0.32
Nodes (4): DirectEntryValidator, decimal, IEnumerable, int

### Community 9 - "Community 9"
Cohesion: 0.29
Nodes (6): net10.0, Microsoft.AspNetCore.OpenApi (10.0.11), Scalar.AspNetCore (2.16.20), WolverineFx (6.26.0), WolverineFx.RuntimeCompilation (6.26.0), Microsoft.NET.Sdk.Web

## Knowledge Gaps
- **22 isolated node(s):** `Program`, `LineMapping`, `PaymentRoutingError`, `Microsoft.AspNetCore.OpenApi (10.0.11)`, `Scalar.AspNetCore (2.16.20)` (+17 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **20 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.