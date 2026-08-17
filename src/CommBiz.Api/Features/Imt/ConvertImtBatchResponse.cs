using CommBiz.Api.Features.Shared;

namespace CommBiz.Api.Features.Imt;

// F-017: duplicates ConvertDirectEntryBatchResponse/ConvertBPayBatchResponse's shape per-slice
// (ADR-002, no shared base type). Mappings (F-021, ADR-009) reuses the shared cross-slice type.
public record ConvertImtBatchResponse(
    bool Success,
    string? ConvertedText,
    IReadOnlyList<PaymentInstructionError>? Errors,
    IReadOnlyList<LineMapping>? Mappings = null);

public record PaymentInstructionError(int Index, string Reason);
