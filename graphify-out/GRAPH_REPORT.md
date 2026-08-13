# Graph Report - src  (2026-08-14)

## Corpus Check
- Corpus is ~1,774 words - fits in a single context window. You may not need a graph.

## Summary
- 104 nodes · 107 edges · 33 communities (12 shown, 21 thin omitted)
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS
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
- Community 17
- Community 19
- Community 20
- Community 24
- Community 25
- Community 26
- Community 27
- Community 28
- Community 29
- Community 30
- Community 31
- Community 32

## God Nodes (most connected - your core abstractions)
1. `CommBiz.Api.Features.DirectEntry` - 12 edges
2. `PaymentInstructionRequest` - 9 edges
3. `DirectEntryValidator` - 7 edges
4. `DirectEntryDetailRecordMapper` - 6 edges
5. `http` - 6 edges
6. `https` - 6 edges
7. `DirectEntrySettings` - 6 edges
8. `DirectEntryHeaderRecordMapper` - 4 edges
9. `DirectEntrySelfBalancingRecordMapper` - 4 edges
10. `DirectEntryTrailerRecordMapper` - 3 edges

## Surprising Connections (you probably didn't know these)
- None detected - all connections are within the same source files.

## Import Cycles
- None detected.

## Communities (33 total, 21 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 1 - "Community 1"
Cohesion: 0.17
Nodes (7): PaymentInstructionRequest, DirectEntryAmountTotals, IReadOnlyList, DirectEntrySelfBalancingRecordMapper, IReadOnlyList, string, IReadOnlyList

### Community 2 - "Community 2"
Cohesion: 0.20
Nodes (6): CommBiz.Api.Features.DirectEntry, DirectEntryTrailerRecordMapper, string, PaymentTypeRouter, string, Program

### Community 3 - "Community 3"
Cohesion: 0.27
Nodes (5): decimal, DirectEntryValidator, IReadOnlyList, IEnumerable, int

### Community 4 - "Community 4"
Cohesion: 0.28
Nodes (5): ConvertDirectEntryBatchCommand, ConvertDirectEntryBatchHandler, ConvertDirectEntryBatchResponse, PaymentInstructionError, IReadOnlyList

### Community 5 - "Community 5"
Cohesion: 0.29
Nodes (4): DirectEntryHeaderRecordMapper, IReadOnlyList, string, DirectEntrySettings

### Community 7 - "Community 7"
Cohesion: 0.40
Nodes (4): net10.0, WolverineFx (6.26.0), WolverineFx.RuntimeCompilation (6.26.0), Microsoft.NET.Sdk.Web

## Knowledge Gaps
- **28 isolated node(s):** `ConvertDirectEntryBatchRequest`, `PingCommand`, `PingHandler`, `PingResult`, `Program` (+23 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **21 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.