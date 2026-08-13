# ADR-002: Vertical slice architecture, one slice per payment type

> Status: ACCEPTED
> Date: 2026-08-13
> Architecture: docs/architecture.md (v1)

## Context
The PRD's roadmap adds further payment types (e.g. international, BPAY) in later tranches, each
converting to its own distinct CommBank format. The codebase needs to support adding a new
payment type without disturbing existing, already-shipped conversions.

## Decision
Organise the codebase as vertical slices — one slice per payment type — where each slice owns
its own request handling, validation, mapping, and output assembly end to end.

## Alternatives Considered

| Option | Why not chosen |
|--------|-----------------|
| Traditional layered architecture (shared mapping layer, shared validation layer, etc.) | Shared horizontal layers risk coupling between payment types as more are added, making each new type's rules harder to isolate |
| A single monolithic handler covering all payment types | Would grow unwieldy as more payment types are added; contradicts the phased-rollout goal |

## Consequences
Each new payment type in a future tranche is added as a new, isolated slice with minimal risk
of regressing existing slices. Some duplication across slices (e.g. similar-looking validation
scaffolding) is accepted as a trade-off for isolation.

## Related
- Architecture section: §2 High-Level Architecture, §3 Direct Entry Conversion Slice
- Supersedes: none
