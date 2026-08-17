# commbiz — Copilot Orientation

> This file is auto-included in every Copilot Chat request in this repo.
> It intentionally holds no decisions, architecture, or status of its
> own — update the linked doc, never this file, when any of that changes.

---

## Identity

- Name: Shaw and Partners → CommBank Payment File Conversion Service (commbiz)
- Owner: <TODO>
- Type: <TODO>

---

## Where to Look

| Topic | Source of truth |
|---|---|
| Problem, goals, scope | `docs/prd.md` |
| System design, components, tech decisions | `docs/architecture.md` |
| Why a specific technical decision was made | `docs/adr/` |
| Feature backlog, phases, status | `docs/project-management.md` |
| Current implementation state, what's next | `docs/handoff.md` |
| Diagrams | `docs/diagrams/mmd/` (source) → `docs/diagrams/img/` (rendered) |

---

## Skill & Agent Path Bindings

> **Note:** These path bindings are static conventions. When running a skill or agent that needs a path,
> check its own documentation for auto-discovery behavior or supply the path explicitly.

### orchestrator-development-pattern (custom agent)
- project-management-doc: `./docs/project-management.md`
- handoff-doc: `./docs/handoff.md`
- source-code-directory: `./src`
- revision-file: `./REVISION.md`
- changelog-file: `./CHANGELOG.md`
- audit-command: `dotnet list package --vulnerable --include-transitive`
- test-runbook-location: `./docs/testing/<FEATURE-ID>-<short-name>.md`
- architecture-doc: `./docs/architecture.md`
- test-cases-doc: `./docs/test-cases.md`
- adr-directory: `./docs/adr`
- diagrams-mmd-dir: `./docs/diagrams/mmd`
- diagrams-img-dir: `./docs/diagrams/img`

### idea-triage (custom agent)
- docs-directory: `./docs`
- prd-file: `./docs/prd.md`
- architecture-file: `./docs/architecture.md`
- project-management-doc: `./docs/project-management.md`
- adr-directory: `./docs/adr`
- diagrams-mmd-dir: `./docs/diagrams/mmd`
- diagrams-img-dir: `./docs/diagrams/img`
- diagram-render-script: `./.github/scripts/render-diagrams.ps1`

**Keep `architecture-file` (idea-triage) and `architecture-doc`
(orchestrator-development-pattern) pointed at the same path** — they're
the same physical file (idea-triage writes it, orchestrator-development-
pattern reads it) but each agent names its variable independently by
design, so either stays usable without the other installed. Same applies
to `adr-directory`, `diagrams-mmd-dir`, and `diagrams-img-dir`.

### graphify
- Graph report: `graphify-out/GRAPH_REPORT.md` (after first run)
- Use before starting any feature that touches multiple modules.
- **This repo has its own scoped update script — prefer it over the generic graphify skill's full
  pipeline.** Run `.github/scripts/graphify-update.ps1` (wraps `graphify_update_run.py`, scoped to
  `src/`, merges into the existing `graphify-out/graph.json`, regenerates `GRAPH_REPORT.md`) from the
  repository root instead of invoking the `graphify` skill directly. Only fall back to the skill's own
  pipeline if this script is missing or fails — the skill's pipeline produces extra artifacts this repo
  doesn't otherwise use (`graph.html`, `cost.json`, `manifest.json`, `.graphify_python/`) at extra
  token/time cost for no benefit here.

### find-skills
- Use to discover installed skills by name or capability.

---

## How Skills and Agents Are Loaded

1. Skills are installed at `.github/skills/<skill-name>/SKILL.md` (workspace) or
   `~/.copilot/skills/<skill-name>/SKILL.md` (personal) — Copilot also reads
   `.claude/skills/` and `.agents/skills/` directly if present.
2. Skills auto-activate when their `description` matches the current request — there is no explicit
   invoke syntax comparable to Claude Code's `/skill-name` or `Skill` tool.
3. Custom agents are installed at `.github/agents/<name>.agent.md` (workspace) or
   `~/.copilot/agents/<name>.agent.md` (personal). Unlike skills, an agent can be switched to directly
   from the agent picker, invoked by name, or delegated to as a subagent by another custom agent that
   lists it under its own `agents:` frontmatter — this is the mechanism `orchestrator-development-pattern`
   relies on to dispatch its five sub-roles.
4. Custom skills: create `.github/skills/<name>/SKILL.md` with your own instructions.
