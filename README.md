# commbiz

Shaw and Partners → CommBank Payment File Conversion Service.

Converts Shaw and Partners' own internal payment instructions (raised on their platform, one
per client payment) into the equivalent CommBank CommBiz file format, ready for submission —
removing the manual/ad-hoc re-keying step that currently sits between raising a payment and
submitting it to the bank.

## What it does

A client posts a batch (JSON array) of payment instructions to a single generic endpoint. The
service figures out the payment type from each instruction, validates every field the CBA spec
requires, and converts a fully valid batch into the matching CommBank file content — returned
inline in the response (no download link, no persistence). An invalid batch is rejected in full,
with a reason per invalid instruction, so nothing is partially converted.

Supported payment types:

| Type | API `paymentTypeCode` | CommBank format |
|---|---|---|
| Direct Entry | `DE` | Fixed-width Direct Entry file (header/detail/self-balancing/trailer) |
| BPAY Batch Payments | `BPAY` | CSV (header + one payment details record per instruction) |
| International Money Transfers | `TT` | MT101-family 27-field CSV |
| Priority Payments (RTGS) | `RTGS` | MT101-family 27-field CSV (shares IMT's format) |
| FX (foreign currency exchange) | `FOREX` | CommBiz IPFX Bulk Settlement Upload CSV |

A batch must be entirely one payment type — mixed or unsupported types reject the whole batch.

## Tech stack

- .NET 10, ASP.NET Core Minimal API, Kestrel only (no IIS/Docker, no database — see
  [ADR-005](docs/adr/ADR-005-kestrel-only-no-database-no-docker.md))
- [Wolverine](https://wolverine.netlify.app/) for in-process command dispatch
- Vertical slice architecture — one folder per payment type under `src/CommBiz.Api/Features/`
  (see [ADR-002](docs/adr/ADR-002-vertical-slice-architecture.md))
- Manual field mapping, no AutoMapper (see [ADR-004](docs/adr/ADR-004-manual-mapping-no-automapper.md))
- xUnit for tests

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Setup and run

Clone the repo, then from the repository root:

```powershell
make up
```

This runs `dotnet watch run --project src/CommBiz.Api --launch-profile http --non-interactive`,
which starts the API with hot reload and opens the Scalar API reference UI in your browser at
`http://localhost:5182/scalar/v1`. Without `make`, run the same command directly, or
`dotnet run --project src/CommBiz.Api`.

The API listens on `http://localhost:5182` by default (see
[Properties/launchSettings.json](src/CommBiz.Api/Properties/launchSettings.json) for the HTTPS
profile).

### Endpoints

| Endpoint | Purpose |
|---|---|
| `GET /health` | Liveness check — returns `{ "status": "Healthy" }` |
| `POST /convert` | Accepts a JSON array of payment instructions, returns the converted file content inline as JSON (`success`, `convertedText`, `mappings`, `errors`) |
| `POST /convert-to-file` | Same routing/validation/conversion as `/convert`, but returns a successful `convertedText` as a downloadable `.txt` file (temporary, see `docs/project-management.md` PM-013) |
| `GET /scalar/v1` | Interactive OpenAPI reference UI |

Example requests for every payment type (happy path and validation-failure scenarios) are in
[tests/smoke/](tests/smoke/) as `.http` files — open any of them in an editor with a REST client
extension and run a request directly against a locally running instance.

## Running tests

```powershell
make test
```

or directly:

```powershell
dotnet test
```

Filter to one payment type's tests with:

```powershell
dotnet test --filter "FullyQualifiedName~DirectEntry"
```

> If `make up`'s `dotnet watch` process is still running, `dotnet test` can fail with a locked
> `CommBiz.Api.exe` file (MSB3027/MSB3021). Stop the running process first:
> `Get-Process -Name "CommBiz.Api" -ErrorAction SilentlyContinue | Stop-Process -Force`

## Configuration

Each payment type's static, organisation-level settings (settlement accounts, remitter names,
lodgement references, etc. — never per-instruction data) live under their own section in
[src/CommBiz.Api/appsettings.json](src/CommBiz.Api/appsettings.json): `DirectEntry`, `BPay`,
`Imt`, `PriorityPayments`, `Fx`.

## Project structure

```
src/CommBiz.Api/Features/     one folder per payment type (BPay, DirectEntry, Fx, Imt,
                               PriorityPayments), plus PaymentRouting (cross-slice dispatch)
                               and Shared (shared field-mapping model/utilities)
tests/CommBiz.Api.Tests/      xUnit unit + endpoint tests, mirroring the Features/ structure
tests/smoke/                  .http example requests per payment type, for manual verification
docs/                         PRD, architecture, ADRs, project management, test cases, runbooks
```

## Documentation

| Topic | Source of truth |
|---|---|
| Problem, goals, scope | [docs/prd.md](docs/prd.md) |
| System design, components, tech decisions | [docs/architecture.md](docs/architecture.md) |
| Why a specific technical decision was made | [docs/adr/](docs/adr/) |
| Feature backlog, phases, status | [docs/project-management.md](docs/project-management.md) |
| Current implementation state, what's next | [docs/handoff.md](docs/handoff.md) |
| Test scenarios | [docs/test-cases.md](docs/test-cases.md) |
| Manual test runbooks | [docs/testing/](docs/testing/) |

Each payment type slice also has its own `README.md` under `src/CommBiz.Api/Features/<Slice>/`
documenting its field mapping, validation rules ("Exception List"), and any sanitisation applied
before validation.