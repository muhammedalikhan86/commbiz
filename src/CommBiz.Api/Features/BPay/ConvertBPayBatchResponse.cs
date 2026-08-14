namespace CommBiz.Api.Features.BPay;

// F-015: duplicates ConvertDirectEntryBatchResponse's shape per-slice (ADR-002, no shared base type).
public record ConvertBPayBatchResponse(bool Success, string? ConvertedText, IReadOnlyList<BPayInstructionError>? Errors);

public record BPayInstructionError(int Index, string Reason);
