# Direct Entry Slice

The Direct Entry vertical slice (ADR-002). Owns its own request model, validation, mapping,
and output-assembly logic for converting Direct Entry payment instructions end to end
(F-003–F-008): request/response contract, Payment Type Router, field validation, detail
record mapping, header/trailer assembly with self-balancing totals, and final fixed-width
file assembly.

Future payment-type slices should follow this same convention: one folder per payment type
under `Features/`.
