---
description: Starts the project locally using whatever run command fits this repo's stack, and verifies it comes up.
name: run
---

# run

## Steps

1. Detect how this project runs by checking, in order: `package.json`
   scripts (`dev`/`start`), a `Makefile` `run` or `up` target, `requirements.txt`
   + an entry point (`app.py`/`main.py`), `Cargo.toml` (`cargo run`),
   `go.mod` (`go run .`), a `.csproj`/`.sln` (`dotnet run`), a
   `Gemfile` (`bin/rails server` or similar). If nothing matches, ask
   the user how they run this project instead of guessing.
2. Check for any required local services this project depends on (e.g.
   a local API, database, or model server mentioned in the README or
   `.github/copilot-instructions.md`) before starting, and warn if one
   isn't reachable. This service has no database and no other service
   dependency by design (see `docs/architecture.md`).
3. Start the app in the background using the detected command.
4. Wait a moment, then confirm it's accessible at whatever port/URL the
   tool reports.
5. Report the URL to the user and note any startup warnings from the
   process output.

## Guardrails

- If no entry point / run target exists yet, stop and tell the user to
  scaffold the app first.
- If dependencies are missing, suggest the stack's install command
  (`dotnet restore`) rather than assuming one.
