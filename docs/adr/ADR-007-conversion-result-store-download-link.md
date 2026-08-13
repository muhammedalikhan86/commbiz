# ADR-007: Temporary in-process Conversion Result Store for download-link delivery

> Status: REJECTED — see ADR-008
> Date: 2026-08-13
> Architecture: docs/architecture.md (v2)

## Context
The user asked whether the API can offer a downloadable link for the converted file, rather
than returning the file content directly inline in the response. The service has no database
(ADR-005), so any mechanism to back a download link must not reintroduce persistent storage.

## Decision
Store a successfully converted file's content temporarily in an in-process cache, keyed by a
short-lived identifier, and return a download link (e.g. `GET /conversions/{id}/file`) in the
conversion response. The cached entry expires after a short time-to-live.

## Alternatives Considered

| Option | Why not chosen |
|--------|-----------------|
| Return the file content directly inline in the response body | Does not satisfy the requested "downloadable link" experience |
| Persist the file to a database or durable store, keyed by id | Would reintroduce persistence, contradicting ADR-005's no-database decision |

## Consequences
The download link only works while the service instance that produced it still holds the
cached entry — if the service runs multiple instances behind a load balancer without sticky
routing, a request for the link could land on a different instance and fail. This trade-off,
and the exact expiry duration, need explicit confirmation before this ADR is accepted.

## Related
- Architecture section: §3 Conversion Result Store, §4 Data Flow, §8 Open Questions & Risks (A1)
- Supersedes: none
- Rejected in favour of: ADR-008 (return output inline as JSON; no download link needed)
