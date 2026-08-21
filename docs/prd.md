# PRD: Shaw and Partners → CommBank Payment File Conversion Service

> Status: APPROVED
> Version: v11
> Last updated: 2026-08-21

## Problem Statement
Shaw and Partners raises individual client payment instructions from its own platform — each
carrying the paying client's account details, the destination account, the amount, the date,
and a payment type indicating which payment method applies (e.g. a domestic transfer, and
potentially other methods such as BPAY or international transfers). Commonwealth Bank's
CommBiz business banking channel only accepts payment/reporting files in its own defined
formats. Today, getting a payment instruction from Shaw and Partners into a state CommBank can
process requires a manual or ad-hoc conversion step, which is slow and error-prone and blocks
straight-through processing.

## Business Context
Advisors raise client payment instructions through a Shaw and Partners portal called **Straight
Through Payments (STP)**, submitted to the Back Office team for processing. A Power Automate
workflow periodically identifies the delta of instructions not yet processed and passes them to
this service as REST calls, receiving the converted CommBank-format content back in response. This
service's responsibility ends there: packaging the returned content into a file, placing that file
in a folder for pickup, and the Back Office team picking it up and posting it to CommBank's
CommBiz portal are all handled outside this service, by the Power Automate workflow and the Back
Office team respectively — not by this product.

## Goals
- Automatically convert a Shaw and Partners payment file into the equivalent CommBank
  submission, with no manual re-keying or manual reformatting step.
- Deliver domestic (Direct Entry) payment conversion first, with further payment types (e.g.
  international, BPAY, foreign currency exchange (FX)) added in later releases, each converted
  into its own corresponding CommBank format.
- Support converting a foreign currency exchange (FX) payment instruction — where Shaw and
  Partners specifies a currency pair being bought and sold — into the equivalent CommBank FX
  settlement submission, so cross-currency payments don't require manual entry into CommBank's
  markets/FX system.
- Support converting a group of payment instructions submitted together in one go, not just
  one at a time — including a run of just a single instruction, since the service itself adds
  the self-balancing entry CommBank's file format requires (see below), rather than relying on
  the size of the submitted run to satisfy that structural rule.
- Ensure every converted file is accepted by CommBank on first submission (no format-related
  rejections), including CommBank's requirement that every file be self-balancing — the service
  adds this balancing entry itself rather than requiring Shaw and Partners to supply one.
- Make failures in conversion (missing or invalid data) visible early, before submission,
  rather than being discovered as a bank-side rejection.
- Reduce the turnaround time between a payment run being raised internally and it being
  submitted to the bank.
- Every conversion also exposes a field-by-field breakdown of the converted output, for the
  convenience of the testing team validating conversions against source data.

## Non-Goals
- Payment types other than Direct Entry are out of scope for the initial release — planned as
  later phases, not excluded permanently.
- Not building a general-purpose "any format to any format" conversion platform — scope is
  limited to the specific Shaw and Partners → CommBank flow.
- Not responsible for approving, authorising, or holding payment mandates — that remains the
  responsibility of existing upstream and downstream systems.
- Not reconciling bank responses/returns back into Shaw and Partners' systems — this product's
  responsibility ends once the converted submission is produced. Ingesting CommBank return/status
  files is out of scope, permanently, not just deferred.
- Not responsible for tracking which instructions have already been processed (the delta of
  unprocessed instructions the calling Power Automate workflow submits), packaging converted
  output into a file, or delivering/posting that file to CommBank's CommBiz portal — these are the
  responsibility of the calling workflow and the Back Office team, not this service (see Business
  Context above).

## User Stories
- As a payments operations user at Shaw and Partners, I want a payment run to be automatically
  converted into the format the bank accepts, so that I don't have to manually reformat or
  re-key it before submission.
- As a payments operations user, I want to be notified when a payment run cannot be converted
  (e.g. missing required data), so that I can fix the source data before it reaches the bank.
- As a payments operations user, I want to submit a foreign currency exchange (FX) payment
  instruction and have it converted into the format CommBank's FX settlement system accepts, so
  that cross-currency payments go through the same straight-through process as domestic ones.
- As a payments operations user, if any single instruction in a submitted group is invalid, I
  want the whole submission rejected with a clear reason per invalid instruction, so that I can
  correct the source data and resubmit with confidence nothing partial went through.
- As a compliance/audit stakeholder, I want every conversion to be traceable back to its source
  payment run, so that we can demonstrate what was submitted and why.

## Success Metrics
- 100% of in-scope payment runs converted without manual intervention.
- 0% bank-side rejections attributable to formatting/conversion errors.
- 100% of invalid submissions rejected in full (no partial conversion), with a validation
  reason returned for every invalid instruction.
- `[TBD]` target turnaround time from source file received to bank-ready file produced.

## Constraints & Assumptions
- The submitted file must conform to whatever CommBank format is confirmed in scope for each
  payment type — this is a fixed external requirement, not something this product can change.
- Conversion is requested on demand for a group of payment instructions submitted together,
  rather than a continuous stream — the exact submission mechanism is an Architecture decision.
- If any instruction in a submitted group is invalid, the entire group is rejected — partial
  conversion is never performed. Valid and invalid instructions are never split across separate
  outcomes.
- Assumes Shaw and Partners' source data contains all information required to populate a
  compliant bank submission; any structural gap is an open question, not an assumption to
  silently work around.
- Every converted Direct Entry file must satisfy CommBank's self-balancing file-structure rule
  (a contra entry offsetting the batch's total, posted against Shaw and Partners' own nominated
  settlement account). This entry is generated by the service itself from configured settlement
  account details — it is never sourced from, or expected in, the Shaw and Partners payload.
- `[TBD]` any regulatory/compliance handling requirements specific to financial payment data
  (e.g. data residency, retention) — to be confirmed.

## Open Questions
| ID | Question | Owner | Resolved? |
|----|----------|-------|-----------|
| Q1 | Which payment type is in scope first? | User | Yes — Direct Entry first; other payment types (routed to their own CommBank format) added in later phases. |
| Q2 | What is the Shaw and Partners source format/system, at a business level? | User | Yes — each instruction is a discrete, structured payment record (source client account, destination account, amount, currency, payment date, payment type, and the staff member who raised it). Exact data shape carried into Architecture. |
| Q3 | What triggers a conversion, and at what volume/cadence? | User | Yes — on demand, for a group of payment instructions submitted together. Exact submission mechanism carried into Architecture. |
| Q4 | Should this product also ingest CommBank's return/status files (which report what happened to a payment after submission — e.g. accepted, or returned/rejected by the receiving bank) and feed that back to Shaw and Partners? Or does this product's responsibility end once the converted submission is handed off to CommBank? | User | Yes — out of scope; this product only converts one format to the other. |
| Q5 | If one payment instruction in a submitted group is invalid, should the whole group be rejected, or should the valid ones still be converted while the invalid one is flagged individually? | User | Yes — whole group is rejected; every invalid instruction gets a validation reason so the user can fix and resubmit. |

## Version History
| Version | Date | Change | Triggered By |
|---------|------|--------|---------------|
| v1 | 2026-08-13 | Initial draft | — |
| v7 | 2026-08-13 | Added CommBank's self-balancing file requirement (service-generated contra entry) as a Goal and Constraint; clarified that a single-instruction run is now fully supported, since the service's own contra entry — not the run's size — satisfies the file's structural minimum | User requirement change |
| v2 | 2026-08-13 | Confirmed business-level shape of Shaw and Partners input (discrete structured payment records with a payment-type indicator); refined Q1 to ask whether v1 scope is domestic-only or multi payment-type; refined Q3 to ask single-instruction vs. batch trigger | Triage edit |
| v3 | 2026-08-13 | Resolved Q1 (Direct Entry first, other types phased in later) and Q3 (on-demand, submitted as a group); added phased-rollout and group-submission goals; added Q5 on partial-batch-failure handling. Webapp/endpoint/output-format implementation details acknowledged but deferred to Architecture per document boundaries | Triage edit |
| v4 | 2026-08-13 | Resolved Q5 (whole-group rejection with per-instruction validation reasons, no partial conversion); added corresponding user story, success metric, and constraint; clarified Q4 with an explanation of what bank return/status files are | Triage edit |
| v5 | 2026-08-13 | Resolved Q4: bank return/status handling is permanently out of scope, not deferred — product is conversion-only. Non-Goals updated accordingly. All open questions resolved | Triage edit |
| v6 | 2026-08-13 | PRD approved | Gate approval |
| v8 | 2026-08-17 | Added Goal: every conversion also exposes a field-by-field breakdown of the converted output, for the testing team's convenience validating conversions against source data | Triage edit — downstream Architecture change (field-level mapping response contract) |
| v9 | 2026-08-18 | Added foreign currency exchange (FX) as a new in-scope payment type: Goal (convert FX instructions into CommBank's FX settlement submission) and corresponding User Story. Confirmed CommBank file specification and source request shape provided; both carried into Architecture, not recorded here per Document Boundaries | Triage edit |
| v10 | 2026-08-18 | PRD approved | Gate approval |
| v11 | 2026-08-21 | Added Business Context section describing the actual operational flow: Advisors raise instructions via the Straight Through Payments (STP) portal to the Back Office team; a Power Automate workflow submits the delta of unprocessed instructions to this service as REST calls and receives converted CommBank-format content back; packaging that content into a file and posting it to CommBank's CommBiz portal is handled by the workflow and Back Office team, not this service. Added a corresponding Non-Goal | User-provided business context |