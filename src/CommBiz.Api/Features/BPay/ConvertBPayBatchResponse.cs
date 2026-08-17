using CommBiz.Api.Features.Shared;

namespace CommBiz.Api.Features.BPay;

// F-015: duplicates ConvertDirectEntryBatchResponse's shape per-slice (ADR-002, no shared base type).
// Mappings (F-021, ADR-009) reuses the shared cross-slice type, unlike Success/ConvertedText/Errors above.
public record ConvertBPayBatchResponse(
    bool Success,
    string? ConvertedText,
    IReadOnlyList<BPayInstructionError>? Errors,
    IReadOnlyList<LineMapping>? Mappings = null);

public record BPayInstructionError(int Index, string Reason);
