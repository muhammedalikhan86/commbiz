# Direct Entry Slice

The Direct Entry vertical slice (ADR-002). Owns its own request model, validation, mapping,
and output-assembly logic for converting Direct Entry payment instructions end to end
(F-003–F-008, F-014): request/response contract, field validation, detail record mapping
(plus the F-014 self-balancing contra record), header/trailer assembly with self-balancing
totals, and final fixed-width file assembly. Dispatched to from the top-level Payment Type
Router (`Features/PaymentRouting`, F-015) — this slice no longer owns routing itself; F-004's
original DirectEntry-local routing check was removed once F-015 centralized it.

Future payment-type slices should follow this same convention: one folder per payment type
under `Features/`.
