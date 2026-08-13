namespace CommBiz.Api.Features.DirectEntry;

// Per ADR-008: converted text is returned inline, not via a download link.
// Success carries ConvertedText; failure carries Errors — F-005 populates Errors, this feature only wires the shape.
public record ConvertDirectEntryBatchResponse(
    bool Success,
    string? ConvertedText,
    IReadOnlyList<PaymentInstructionError>? Errors);

public record PaymentInstructionError(int Index, string Reason);
