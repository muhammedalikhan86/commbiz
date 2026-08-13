# ADR-008: Return converted output inline as text within a JSON response

> Status: ACCEPTED
> Date: 2026-08-13
> Architecture: docs/architecture.md (v3)

## Context
ADR-007 proposed a download-link mechanism backed by a temporary in-process cache, to satisfy
a request for downloadable output. On review, this conflicted with the goal of running
multiple stateless instances without coordination (NFR-005), and was reconsidered in favour of
a simpler approach: the caller does not need a separate download step at all.

## Decision
Return the assembled Direct Entry file content directly, as text within the JSON response body
of the conversion request, alongside a success indicator. No download link, no server-side
result caching.

## Alternatives Considered

| Option | Why not chosen |
|--------|-----------------|
| Download link backed by a temporary cache (ADR-007) | Rejected — reintroduces per-instance state and risks link failures in a multi-instance deployment |
| Persist the result and expose it via a separate retrieval endpoint | Reintroduces persistence, contradicting the no-database decision (ADR-005) |

## Consequences
The service remains fully stateless (NFR-004/NFR-005 hold without caveats). The response
payload size grows with the batch size, since the full converted text is returned inline; this
is acceptable given the expected batch sizes are small on-demand submissions, not bulk files.

## Related
- Architecture section: §3 API Host, §4 Data Flow, §8 Open Questions & Risks (A1)
- Supersedes: none (ADR-007 was never ACCEPTED, so this is a fresh decision, not a supersession)
