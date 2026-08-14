namespace CommBiz.Api.Features.BPay;

public record ConvertBPayBatchCommand(IReadOnlyList<BPayPaymentInstructionRequest> Instructions);

public static class ConvertBPayBatchHandler
{
    // F-016: validate -> map details -> map header -> assemble. Header + Details only - BPay has no
    // trailer/self-balancing record (docs/stash/BPay Payments - CommBiz File Specification.md).
    public static ConvertBPayBatchResponse Handle(ConvertBPayBatchCommand command, BPaySettings settings)
    {
        var instructions = command.Instructions;

        var validationErrors = BPayValidator.Validate(instructions);
        if (validationErrors is not null)
        {
            return new ConvertBPayBatchResponse(false, null, validationErrors);
        }

        var detailRecords = string.Concat(
            instructions.Select(instruction => BPayDetailRecordMapper.Map(instruction) + "\r\n"));

        var convertedText =
            BPayHeaderRecordMapper.Map(instructions, settings) + "\r\n" +
            detailRecords;

        return new ConvertBPayBatchResponse(true, convertedText, null);
    }
}
