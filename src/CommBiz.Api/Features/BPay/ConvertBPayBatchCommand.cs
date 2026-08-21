using CommBiz.Api.Features.Shared;

namespace CommBiz.Api.Features.BPay;

public record ConvertBPayBatchCommand(IReadOnlyList<BPayPaymentInstructionRequest> Instructions);

public static class ConvertBPayBatchHandler
{
    // F-016: validate -> map details -> map header -> assemble. Header + Details only - BPay has no
    // trailer/self-balancing record (docs/stash/BPay Payments - CommBiz File Specification.md).
    public static ConvertBPayBatchResponse Handle(ConvertBPayBatchCommand command, BPaySettings settings, TimeProvider timeProvider)
    {
        var instructions = command.Instructions;

        var validationErrors = BPayValidator.Validate(instructions, timeProvider);
        if (validationErrors is not null)
        {
            return new ConvertBPayBatchResponse(false, null, validationErrors);
        }

        var detailRecords = string.Concat(
            instructions.Select(instruction => BPayDetailRecordMapper.Map(instruction) + "\r\n"));

        // F-021: resolved once and passed to both Map and MapFields below so ConvertedText and Mappings
        // can never disagree on the header's File Creation Date/Time (AC5).
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var convertedText =
            BPayHeaderRecordMapper.Map(instructions, settings, now) + "\r\n" +
            detailRecords;

        // F-021: Mappings mirrors ConvertedText's line order exactly - header, then one per instruction.
        var mappings = new List<LineMapping>
        {
            new("header", BPayHeaderRecordMapper.MapFields(instructions, settings, now)),
        };
        mappings.AddRange(instructions.Select(
            (instruction, index) => new LineMapping($"detail{index + 1}", BPayDetailRecordMapper.MapFields(instruction))));

        return new ConvertBPayBatchResponse(true, convertedText, null, mappings);
    }
}
