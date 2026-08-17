using CommBiz.Api.Features.Shared;

namespace CommBiz.Api.Features.DirectEntry;

// Per ADR-008: converted text is returned inline, not via a download link.
// Success carries ConvertedText; failure carries Errors — F-005 populates Errors, this feature only wires the shape.
// Mappings (F-021, ADR-009) is strictly additive/parallel to ConvertedText, never a replacement for it.
public record ConvertDirectEntryBatchResponse(
    bool Success,
    string? ConvertedText,
    IReadOnlyList<PaymentInstructionError>? Errors,
    IReadOnlyList<LineMapping>? Mappings = null);

public record PaymentInstructionError(int Index, string Reason);
