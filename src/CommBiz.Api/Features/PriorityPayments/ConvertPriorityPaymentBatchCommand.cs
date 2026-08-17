using CommBiz.Api.Features.Shared;

namespace CommBiz.Api.Features.PriorityPayments;

public record ConvertPriorityPaymentBatchCommand(IReadOnlyList<PriorityPaymentInstructionRequest> Instructions);

public static class ConvertPriorityPaymentBatchHandler
{
    // F-018: validate -> map each instruction to its 27-field CSV row -> join with CRLF. Same shape
    // as IMT - no header/trailer record, and no trailing CRLF after the last row.
    public static ConvertPriorityPaymentBatchResponse Handle(ConvertPriorityPaymentBatchCommand command, PriorityPaymentsSettings settings)
    {
        var instructions = command.Instructions;

        var validationErrors = PriorityPaymentValidator.Validate(instructions);
        if (validationErrors is not null)
        {
            return new ConvertPriorityPaymentBatchResponse(false, null, validationErrors);
        }

        // Batch-invariant, derived once and reused across every row rather than recomputed per instruction.
        var debitAccountNumber = PriorityPaymentRecordMapper.DeriveDebitAccountNumber(settings);

        var convertedText = string.Join(
            "\r\n",
            instructions.Select(instruction => PriorityPaymentRecordMapper.Map(instruction, debitAccountNumber)));

        // Mappings mirrors ConvertedText's row order exactly - one row per instruction, no header/trailer.
        var mappings = instructions
            .Select((instruction, index) => new LineMapping(
                $"row{index + 1}", PriorityPaymentRecordMapper.MapFields(instruction, debitAccountNumber)))
            .ToList();

        return new ConvertPriorityPaymentBatchResponse(true, convertedText, null, mappings);
    }
}
