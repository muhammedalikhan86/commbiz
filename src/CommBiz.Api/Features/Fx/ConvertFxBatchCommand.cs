using CommBiz.Api.Features.Shared;

namespace CommBiz.Api.Features.Fx;

public record ConvertFxBatchCommand(IReadOnlyList<FxPaymentInstructionRequest> Instructions);

public static class ConvertFxBatchHandler
{
    // F-023: validate -> map each instruction to its 27-field CSV row -> join with CRLF. Same shape
    // as IMT/PP - no header/trailer record, and no trailing CRLF after the last row.
    public static ConvertFxBatchResponse Handle(ConvertFxBatchCommand command, FxSettings settings)
    {
        var instructions = command.Instructions;

        var validationErrors = FxValidator.Validate(instructions);
        if (validationErrors is not null)
        {
            return new ConvertFxBatchResponse(false, null, validationErrors);
        }

        var convertedText = string.Join(
            "\r\n",
            instructions.Select(instruction => FxRecordMapper.Map(instruction, settings)));

        // Mappings mirrors ConvertedText's row order exactly - one row per instruction, no header/trailer.
        var mappings = instructions
            .Select((instruction, index) => new LineMapping($"row{index + 1}", FxRecordMapper.MapFields(instruction, settings)))
            .ToList();

        return new ConvertFxBatchResponse(true, convertedText, null, mappings);
    }
}
