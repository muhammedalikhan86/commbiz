# ADR-004: Manual object mapping, no AutoMapper or other commercial mapping library

> Status: ACCEPTED
> Date: 2026-08-13
> Architecture: docs/architecture.md (v1)

## Context
Converting a payment instruction into a Direct Entry detail record is a small, fixed mapping
(a handful of fields into fixed-width positions) rather than a large, evolving object graph.

## Decision
Map payment instructions to Direct Entry records with plain, explicit C# mapping code — no
AutoMapper or other commercial/general-purpose mapping library.

## Alternatives Considered

| Option | Why not chosen |
|--------|-----------------|
| AutoMapper | Directed choice to avoid commercial NuGet packages; the mapping is small and fixed enough that a library adds indirection without real benefit |
| Other general-purpose mapping libraries | Same reasoning — not justified for a small, fixed, per-slice mapping |

## Consequences
Mapping logic is explicit and easy to follow directly in the code, at the cost of writing
boilerplate mapping code by hand for each payment type's slice as they're added.

## Related
- Architecture section: §3 Direct Entry Conversion Slice
- Supersedes: none
