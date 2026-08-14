namespace CommBiz.Api.Features.DirectEntry;

public record ConvertDirectEntryBatchCommand(IReadOnlyList<PaymentInstructionRequest> Instructions);

public static class ConvertDirectEntryBatchHandler
{
    public static ConvertDirectEntryBatchResponse Handle(
        ConvertDirectEntryBatchCommand command, DirectEntrySettings settings)
    {
        var instructions = command.Instructions;

        // Payment-type routing (unsupported/mixed types) is enforced once, at the batch level, by the
        // top-level cross-slice router (Features/PaymentRouting) before this handler ever runs.
        var validationErrors = DirectEntryValidator.Validate(instructions);
        if (validationErrors is not null)
        {
            return new ConvertDirectEntryBatchResponse(false, null, validationErrors);
        }

        var detailRecords = string.Concat(
            instructions.Select(instruction => DirectEntryDetailRecordMapper.Map(instruction, settings) + "\r\n"));

        // Header + Details (F-006) + self-balancing contra record (F-014, immediately before the
        // trailer, never interleaved with real details) + Trailer (F-007).
        var convertedText =
            DirectEntryHeaderRecordMapper.Map(instructions, settings) + "\r\n" +
            detailRecords +
            DirectEntrySelfBalancingRecordMapper.Map(instructions, settings) + "\r\n" +
            DirectEntryTrailerRecordMapper.Map(instructions, settings) + "\r\n";

        return new ConvertDirectEntryBatchResponse(true, convertedText, null);
    }
}
