# ADR-006: Shaw.Diagnostics (in-house NuGet) for logging/observability

> Status: ACCEPTED
> Date: 2026-08-13
> Architecture: docs/architecture.md (v2)

## Context
The PRD requires that every conversion be traceable back to its source payment run (audit/
compliance user story), but the service holds no database. Logging is therefore the primary
mechanism for satisfying that traceability requirement, and the organisation already maintains
an in-house diagnostics package for this purpose.

## Decision
Use Shaw.Diagnostics (in-house NuGet package), version 2.0.0, sourced from the internal feed at
`\\sic2tfs1\nuget\Packages`, for logging/observability.

## Alternatives Considered

| Option | Why not chosen |
|--------|-----------------|
| Built-in `ILogger`/`Microsoft.Extensions.Logging` only | Directed choice to use the organisation's existing in-house package instead |
| A third-party logging library (e.g. Serilog) | Same reasoning — the in-house package is the standard for this organisation |

## Consequences
The service depends on an internal NuGet feed being reachable at build time. Detailed logging
design (what gets logged, redaction of sensitive fields per NFR-002, correlation IDs) is left
for implementation, not specified further here.

## Related
- Architecture section: §6 NFR-002, §8 Open Questions & Risks (A5)
- Supersedes: none
