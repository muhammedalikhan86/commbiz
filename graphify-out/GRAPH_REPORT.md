# Graph Report - src  (2026-08-18)

## Corpus Check
- Corpus is ~9,717 words - fits in a single context window. You may not need a graph.

## Summary
- 285 nodes · 499 edges · 21 communities (11 shown, 10 thin omitted)
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
- Community 15
- Community 16
- Community 17
- Community 18
- Community 19
- Community 20

## God Nodes (most connected - your core abstractions)
1. `CommBiz.Api.Features.Shared` - 21 edges
2. `PaymentInstructionRequest` - 15 edges
3. `PaymentTypeRouter` - 13 edges
4. `DirectEntrySettings` - 12 edges
5. `PriorityPaymentValidator` - 12 edges
6. `ImtValidator` - 12 edges
7. `CommBiz.Api.Features.DirectEntry` - 11 edges
8. `FieldMapping` - 10 edges
9. `ImtRecordMapper` - 10 edges
10. `PriorityPaymentRecordMapper` - 8 edges

## Surprising Connections (you probably didn't know these)
- None detected - all connections are within the same source files.

## Import Cycles
- None detected.

## Communities (21 total, 10 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.08
Nodes (26): AmountTotalField, AmountTotalInCents, CommBiz.Api.Features.DirectEntry, DateToBeProcessed, DescriptionOfEntriesOnFile, PaymentInstructionRequest, DirectEntryAmountTotals, IReadOnlyList (+18 more)

### Community 1 - "Community 1"
Cohesion: 0.08
Nodes (18): CommBiz.Api.Features.Imt, ConvertImtBatchCommand, ConvertImtBatchHandler, ConvertImtBatchResponse, PaymentInstructionError, ImtPaymentInstructionRequest, ImtRecordMapper, GeneratedRegex (+10 more)

### Community 2 - "Community 2"
Cohesion: 0.11
Nodes (19): CommBiz.Api.Features.BPay, BPayHeaderRecordMapper, DateTime, IReadOnlyList, string, TotalAmountInCents, BPayPaymentInstructionRequest, BPaySettings (+11 more)

### Community 3 - "Community 3"
Cohesion: 0.10
Nodes (17): CommBiz.Api.Features.Fx, ConvertFxBatchCommand, ConvertFxBatchHandler, ConvertFxBatchResponse, PaymentInstructionError, FxPaymentInstructionRequest, FxRecordMapper, IReadOnlyList (+9 more)

### Community 4 - "Community 4"
Cohesion: 0.13
Nodes (11): CommBiz.Api.Features.Shared, BPayDetailRecordMapper, IReadOnlyList, string, DirectEntryDetailRecordMapper, IReadOnlyList, string, PaymentInstructionError (+3 more)

### Community 5 - "Community 5"
Cohesion: 0.14
Nodes (11): CommBiz.Api.Features.PriorityPayments, ConvertPriorityPaymentBatchCommand, ConvertPriorityPaymentBatchHandler, ConvertPriorityPaymentBatchResponse, PriorityPaymentInstructionRequest, PriorityPaymentRecordMapper, GeneratedRegex, IReadOnlyList (+3 more)

### Community 6 - "Community 6"
Cohesion: 0.22
Nodes (11): CommBiz.Api.Features.PaymentRouting, PaymentRoutingError, PaymentRoutingResponse, PaymentTypeRouter, int, string, IMessageBus, IResult (+3 more)

### Community 7 - "Community 7"
Cohesion: 0.12
Nodes (17): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, launchUrl, applicationUrl (+9 more)

### Community 8 - "Community 8"
Cohesion: 0.15
Nodes (9): ConvertDirectEntryBatchCommand, ConvertDirectEntryBatchHandler, ConvertDirectEntryBatchResponse, PaymentInstructionError, DirectEntryValidator, decimal, IEnumerable, int (+1 more)

### Community 9 - "Community 9"
Cohesion: 0.21
Nodes (8): PriorityPaymentValidator, DateTime, decimal, GeneratedRegex, IEnumerable, int, IReadOnlyList, Regex

### Community 10 - "Community 10"
Cohesion: 0.29
Nodes (6): net10.0, Microsoft.AspNetCore.OpenApi (10.0.11), Scalar.AspNetCore (2.16.20), WolverineFx (6.26.0), WolverineFx.RuntimeCompilation (6.26.0), Microsoft.NET.Sdk.Web

## Knowledge Gaps
- **22 isolated node(s):** `LineMapping`, `PaymentRoutingError`, `Program`, `applicationUrl`, `commandName` (+17 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **10 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.