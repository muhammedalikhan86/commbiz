namespace CommBiz.Api.Features.Imt;

// F-017: duplicates ConvertDirectEntryBatchResponse/ConvertBPayBatchResponse's shape per-slice
// (ADR-002, no shared base type).
public record ConvertImtBatchResponse(bool Success, string? ConvertedText, IReadOnlyList<PaymentInstructionError>? Errors);

public record PaymentInstructionError(int Index, string Reason);
