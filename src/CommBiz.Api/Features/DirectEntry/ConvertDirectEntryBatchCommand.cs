namespace CommBiz.Api.Features.DirectEntry;

public record ConvertDirectEntryBatchCommand(ConvertDirectEntryBatchRequest Request);

public static class ConvertDirectEntryBatchHandler
{
    public static ConvertDirectEntryBatchResponse Handle(ConvertDirectEntryBatchCommand command)
    {
        var request = command.Request;

        var unsupportedTypeErrors = PaymentTypeRouter.FindUnsupportedPaymentTypes(request.Instructions);
        if (unsupportedTypeErrors is not null)
        {
            return new ConvertDirectEntryBatchResponse(false, null, unsupportedTypeErrors);
        }

        var validationErrors = DirectEntryValidator.Validate(request);
        if (validationErrors is not null)
        {
            return new ConvertDirectEntryBatchResponse(false, null, validationErrors);
        }

        var detailRecords = string.Concat(
            request.Instructions.Select(instruction => DirectEntryDetailRecordMapper.Map(instruction) + "\r\n"));

        // Header + Details (F-006, unchanged) + Trailer (F-007); final short-form nuances are F-008's job.
        var convertedText =
            DirectEntryHeaderRecordMapper.Map(request) + "\r\n" +
            detailRecords +
            DirectEntryTrailerRecordMapper.Map(request.Instructions) + "\r\n";

        return new ConvertDirectEntryBatchResponse(true, convertedText, null);
    }
}
