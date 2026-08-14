namespace CommBiz.Api.Features.Imt;

public record ConvertImtBatchCommand(IReadOnlyList<ImtPaymentInstructionRequest> Instructions);

public static class ConvertImtBatchHandler
{
    // F-017: validate -> map each instruction to its 27-field CSV row -> join with CRLF. Unlike Direct
    // Entry/BPAY, the IMT spec explicitly forbids a trailing CRLF after the last row (§1.2 rule 2) and
    // has no header/trailer record of its own - do not "fix" this later to match the other two slices.
    public static ConvertImtBatchResponse Handle(ConvertImtBatchCommand command, ImtSettings settings)
    {
        var instructions = command.Instructions;

        var validationErrors = ImtValidator.Validate(instructions);
        if (validationErrors is not null)
        {
            return new ConvertImtBatchResponse(false, null, validationErrors);
        }

        // Batch-invariant, derived once and reused across every row rather than recomputed per instruction.
        var debitAccountNumber = ImtRecordMapper.DeriveDebitAccountNumber(settings);

        var convertedText = string.Join(
            "\r\n",
            instructions.Select(instruction => ImtRecordMapper.Map(instruction, debitAccountNumber)));

        return new ConvertImtBatchResponse(true, convertedText, null);
    }
}
