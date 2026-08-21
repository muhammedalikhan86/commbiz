using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Wolverine;

namespace CommBiz.Api.Features.PaymentRouting;

// TEMPORARY: /convert-to-file support. Reuses PaymentTypeRouter's routing/dispatch as-is and only
// re-shapes the successful result — ConvertedText as a downloadable .txt, nothing else from the
// response object. Failure/rejection cases fall back to the same JSON body /convert would return,
// since there is no ConvertedText to write to a file.
public static class ConvertToFileRouter
{
    public static async Task<IResult> RouteAndDispatchAsync(JsonElement body, IMessageBus bus, TimeProvider timeProvider)
    {
        var result = await PaymentTypeRouter.RouteAndDispatchAsync(body, bus);
        var (success, convertedText) = result is IValueHttpResult valueResult
            ? ExtractConvertedText(valueResult.Value)
            : (false, null);

        if (!success || convertedText is null)
        {
            return result;
        }

        var fileName = BuildFileName(body, timeProvider);
        return Results.File(Encoding.UTF8.GetBytes(convertedText), "text/plain", fileName);
    }

    private static string BuildFileName(JsonElement body, TimeProvider timeProvider)
    {
        var paymentTypeCode = body.ValueKind == JsonValueKind.Array && body.GetArrayLength() > 0
            ? PaymentTypeRouter.GetPaymentTypeCode(body[0])
            : null;
        var safePaymentTypeCode = IsSafeFileNameSegment(paymentTypeCode) ? paymentTypeCode : "convert";
        var timestamp = timeProvider.GetUtcNow().ToString("yyyyMMdd-HHmm-ssfff");
        return $"{safePaymentTypeCode}-{timestamp}.txt";
    }

    // paymentTypeCode is attacker-controlled JSON content; only allow it into the response filename
    // if it's plain alphanumeric, never passing raw/unsanitized external input into an HTTP header value.
    private static bool IsSafeFileNameSegment(string? value) =>
        !string.IsNullOrEmpty(value) && value.All(char.IsLetterOrDigit);

    private static (bool Success, string? ConvertedText) ExtractConvertedText(object? value) => value switch
    {
        DirectEntry.ConvertDirectEntryBatchResponse r => (r.Success, r.ConvertedText),
        BPay.ConvertBPayBatchResponse r => (r.Success, r.ConvertedText),
        Imt.ConvertImtBatchResponse r => (r.Success, r.ConvertedText),
        PriorityPayments.ConvertPriorityPaymentBatchResponse r => (r.Success, r.ConvertedText),
        Fx.ConvertFxBatchResponse r => (r.Success, r.ConvertedText),
        PaymentRoutingResponse r => (r.Success, r.ConvertedText),
        _ => (false, null),
    };
}
