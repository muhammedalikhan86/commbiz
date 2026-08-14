namespace CommBiz.Api.Features.PaymentRouting;

// Router-level rejection response (F-015): used only for batch-level rejections (empty, mixed,
// unsupported type) that happen before any slice is dispatched to. Deliberately duplicates the
// same tiny shape each slice's own response uses (ADR-002) rather than sharing a base type.
public record PaymentRoutingResponse(bool Success, string? ConvertedText, IReadOnlyList<PaymentRoutingError>? Errors);

public record PaymentRoutingError(int Index, string Reason);
