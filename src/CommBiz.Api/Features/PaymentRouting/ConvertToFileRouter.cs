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
    public static async Task<IResult> RouteAndDispatchAsync(JsonElement body, IMessageBus bus)
    {
        var result = await PaymentTypeRouter.RouteAndDispatchAsync(body, bus);
        var (success, convertedText) = result is IValueHttpResult valueResult
            ? ExtractConvertedText(valueResult.Value)
            : (false, null);

        if (!success || convertedText is null)
        {
            return result;
        }

        var fileName = BuildFileName(body);
        return Results.File(Encoding.UTF8.GetBytes(convertedText), "text/plain", fileName);
    }

    private static string BuildFileName(JsonElement body)
    {
        var paymentTypeCode = body.ValueKind == JsonValueKind.Array && body.GetArrayLength() > 0
            ? PaymentTypeRouter.GetPaymentTypeCode(body[0])
            : null;
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmm-ssfff");
        return $"{paymentTypeCode ?? "convert"}-{timestamp}.txt";
    }

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
