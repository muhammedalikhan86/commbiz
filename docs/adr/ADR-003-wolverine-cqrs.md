# ADR-003: Wolverine for in-process CQRS/command dispatch

> Status: ACCEPTED
> Date: 2026-08-13
> Architecture: docs/architecture.md (v1)

## Context
Each vertical slice needs a consistent way to receive a request, run it through validation, and
dispatch it to its conversion logic, without hand-rolling dispatch/pipeline code in every slice.

## Decision
Use Wolverine as the in-process command/handler pipeline for dispatching requests to each
slice's conversion logic.

## Alternatives Considered

| Option | Why not chosen |
|--------|-----------------|
| MediatR | Directed choice — Wolverine specified explicitly in place of a MediatR-style mediator |
| Hand-rolled dispatch per slice | Would duplicate pipeline concerns (e.g. validation ordering) across every slice as more payment types are added |

## Consequences
Slices get a consistent request → validate → handle pipeline via Wolverine's conventions. The
team takes on a dependency on Wolverine's routing/handler discovery conventions instead of an
explicit, hand-written dispatch table.

## Related
- Architecture section: §2 High-Level Architecture, §3 Payment Type Router
- Supersedes: none
