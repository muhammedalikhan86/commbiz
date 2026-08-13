# ADR-001: .NET 10 with ASP.NET Core Minimal API as the service host

> Status: ACCEPTED
> Date: 2026-08-13
> Architecture: docs/architecture.md (v1)

## Context
The service is a small, focused conversion endpoint: accept a batch of payment instructions,
validate and convert them, return a result. It has no need for MVC-style controllers, views, or
the broader ceremony of a full web framework.

## Decision
Build the service on .NET 10 using ASP.NET Core Minimal API.

## Alternatives Considered

| Option | Why not chosen |
|--------|-----------------|
| ASP.NET Core MVC (controller-based) | Adds controller/routing ceremony not needed for a small, single-purpose conversion API |
| A non-.NET stack | Directed choice — .NET 10 specified explicitly |

## Consequences
Minimal API keeps the endpoint surface small and easy to reason about. Some conventions
(e.g. filters, model binding customisation) work slightly differently than in MVC, which the
team should be aware of when extending the API surface for future payment types.

## Related
- Architecture section: §2 High-Level Architecture, §3 API Host
- Supersedes: none
