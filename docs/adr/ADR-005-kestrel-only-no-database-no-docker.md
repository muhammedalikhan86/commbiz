# ADR-005: Self-hosted on Kestrel directly — no database, no containerization

> Status: ACCEPTED
> Date: 2026-08-13
> Architecture: docs/architecture.md (v1)

## Context
The service is a stateless conversion step: it holds no data between requests and has no
persistence requirement. Deployment simplicity is preferred over infrastructure that isn't
needed for a stateless, in-memory-only workload.

## Decision
Run the service directly on Kestrel with no database and no containerization (no Docker).

## Alternatives Considered

| Option | Why not chosen |
|--------|-----------------|
| Containerized deployment (Docker) | Directed choice to avoid container infrastructure for a service simple enough to run directly on Kestrel |
| A database for storing request/conversion history | Directed choice — service is intentionally stateless; no persistence requirement identified in the PRD |

## Consequences
Deployment and operations stay simple (no container build/registry, no database
schema/migrations to manage). This does mean there is no built-in persisted audit trail — the
PRD's traceability user story must be satisfied another way (see Architecture Open Questions,
A5), since there's no database to record conversions in.

## Related
- Architecture section: §2 High-Level Architecture, §6 NFR-004/NFR-005
- Supersedes: none
