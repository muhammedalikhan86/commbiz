# Payment Type Router

Cross-slice component (ADR-002), not a vertical slice itself. `POST /convert`'s only job is to
read the raw JSON batch and hand it here; this router peeks each instruction's `paymentTypeCode`,
rejects the batch outright if it's empty, mixes payment types, or declares a type that isn't wired
yet, then deserializes the raw JSON into the matching slice's own request shape and dispatches to
that slice's own Wolverine command untouched.

Currently wired: `DE` (Direct Entry), `BPAY`, and `TT` (Shaw and Partners' internal "Telegraphic
Transfer" code, dispatched to the IMT slice — the file's own Transaction Type field always writes
the literal `"IMT"`, regardless of this routing code's name). `PP` (Priority Payments, also known
as RTGS) is not yet wired — its request shape hasn't been confirmed by the business (F-018,
tracked as PM-006) — so a batch declaring it is rejected the same way as any other unsupported type.

Each slice still owns its own request/response/validator/mapper end to end; this component only
owns the dispatch decision itself.
