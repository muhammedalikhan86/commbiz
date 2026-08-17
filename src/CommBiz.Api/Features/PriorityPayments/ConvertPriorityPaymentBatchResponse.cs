using CommBiz.Api.Features.Shared;

namespace CommBiz.Api.Features.PriorityPayments;

// F-018: duplicates ConvertImtBatchResponse's shape per-slice (ADR-002, no shared base type).
// Mappings (F-021, ADR-009) reuses the shared cross-slice type, built in from day one - not retrofitted.
public record ConvertPriorityPaymentBatchResponse(
    bool Success,
    string? ConvertedText,
    IReadOnlyList<PaymentInstructionError>? Errors,
    IReadOnlyList<LineMapping>? Mappings = null);

public record PaymentInstructionError(int Index, string Reason);
