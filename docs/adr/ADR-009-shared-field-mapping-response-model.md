# ADR-009: Shared field-mapping response model across all conversion slices

> Status: ACCEPTED
> Date: 2026-08-17
> Architecture: docs/architecture.md (v8)

## Context
The testing team validates each conversion by comparing the assembled fixed-width/CSV output
back against the source payment instructions, which today means manually parsing raw text.
Every conversion slice (Direct Entry, BPay, IMT, and the not-yet-built Priority Payments) needs
an identical structure to expose, for every line it assembles, which request/config field
produced which CBA output field and value. ADR-002 (vertical slice architecture) treats any
cross-slice sharing as an explicit exception requiring its own justification — already granted
once, for the Payment Type Router.

## Decision
Introduce one shared pair of records, `FieldMapping` (RequestField, RequestValue,
CbaResponseField, CbaResponseValue) and `LineMapping` (Line, Fields), reused unmodified by every
conversion slice. Each slice's response returns an ordered `Mappings` list of `LineMapping`
entries — one per line of its assembled output (e.g. `header`, `detail1`, `detail2`,
`selfbalancing`, `trailer` for Direct Entry) — parallel to the existing `ConvertedText` field,
never replacing it. Lists are used at both levels (lines, and fields within a line), not
dictionaries, so line order (matching the assembled file) and field order (matching the spec)
are preserved deterministically.

## Alternatives Considered

| Option | Why not chosen |
|--------|-----------------|
| Duplicate `FieldMapping`/`LineMapping` types per slice | The shape has no per-payment-type variation; four identical types would only add maintenance overhead with no decoupling benefit |
| Flat `Dictionary<string, string>` (field name → value) | Can't represent both sides of the mapping (request-side origin vs. CBA response field/value) that the testing team asked for |
| `Dictionary<string, LineMapping>` keyed by line name | JSON object/dictionary key order is not a reliable guarantee; an ordered list keeps line order explicit and matches how testers read the file top-to-bottom |

## Consequences
This is a second sanctioned exception to ADR-002's no-shared-layer rule. Every already-`Done`
conversion slice (Direct Entry, BPay, IMT) needs a retrofit so its mapper emits this breakdown
alongside the raw text it already builds, from the same field values — not a rewrite of the
mapping logic itself. The still-`Planned` Priority Payments slice (F-018) is built with this from
the start, avoiding a second retrofit. Response payload size grows further (already an accepted
trade-off per ADR-008).

## Related
- Architecture section: §3 Field Mapping Model, §4 Data Flow (step 6), §5 FR-009, §7 Technology
  Decisions
- Supersedes: none
