namespace CommBiz.Api.Features.Shared;

// Shared across Direct Entry, BPay, and IMT (ADR-009) - a second sanctioned cross-slice exception to
// ADR-002, alongside the Payment Type Router. Reused unmodified; never duplicated per slice.
public record FieldMapping(
    string RequestField,
    string? RequestValue,
    string CbaResponseField,
    string? CbaResponseValue);

public record LineMapping(string Line, IReadOnlyList<FieldMapping> Fields);
