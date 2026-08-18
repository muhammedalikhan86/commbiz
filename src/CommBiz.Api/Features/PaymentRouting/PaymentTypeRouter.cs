using System.Text.Json;
using Wolverine;

namespace CommBiz.Api.Features.PaymentRouting;

// Payment Type Router (F-015, architecture.md §3 "Payment Type Router" / §2): the real, top-level
// cross-slice dispatcher. Peeks paymentTypeCode on the raw JSON batch, rejects at the batch level
// (empty / mixed / unsupported), then deserializes into the matching slice's own request shape and
// dispatches to that slice's own Wolverine command. Not a vertical slice itself (ADR-002) — this is
// the one genuinely shared, cross-slice component.
public static class PaymentTypeRouter
{
    private const int HeaderErrorIndex = -1;
    private const string DirectEntryType = "DE";
    private const string BPayType = "BPAY";
    private const string ImtType = "TT"; // F-017: Shaw and Partners' internal "Telegraphic Transfer" code - CBA's file calls this format "IMT"
    private const string PriorityPaymentType = "RTGS"; // F-018: Shaw and Partners' routing code - never written to output, which always uses the literal "PP"
    private const string FxType = "FOREX"; // F-022: dispatches to the FX Conversion Slice (F-023)
    private const string PaymentTypeCodeProperty = "paymentTypeCode";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<IResult> RouteAndDispatchAsync(JsonElement body, IMessageBus bus)
    {
        if (body.ValueKind != JsonValueKind.Array || body.GetArrayLength() == 0)
        {
            return Results.Ok(Rejected(
                "Payment batch must contain at least 1 payment instruction; unable to determine payment type."));
        }

        var instructions = body.EnumerateArray().ToArray();
        var typeCodes = instructions.Select(GetPaymentTypeCode).ToArray();
        var distinctTypes = typeCodes.Select(code => code?.ToUpperInvariant() ?? string.Empty).Distinct().ToArray();

        if (distinctTypes.Length > 1)
        {
            return Results.Ok(Rejected(
                $"Payment batch must not mix payment types (found {string.Join(", ", distinctTypes.Select(t => $"'{t}'"))})."));
        }

        return distinctTypes[0] switch
        {
            DirectEntryType => await DispatchDirectEntryAsync(body, bus),
            BPayType => await DispatchBPayAsync(body, bus),
            ImtType => await DispatchImtAsync(body, bus),
            PriorityPaymentType => await DispatchPriorityPaymentAsync(body, bus),
            FxType => await DispatchFxAsync(body, bus),
            _ => Results.Ok(RejectedUnsupported(typeCodes))
        };
    }

    private static async Task<IResult> DispatchDirectEntryAsync(JsonElement body, IMessageBus bus)
    {
        var instructions = body.Deserialize<List<DirectEntry.PaymentInstructionRequest>>(JsonOptions) ?? [];
        var response = await bus.InvokeAsync<DirectEntry.ConvertDirectEntryBatchResponse>(
            new DirectEntry.ConvertDirectEntryBatchCommand(instructions));
        return Results.Ok(response);
    }

    private static async Task<IResult> DispatchBPayAsync(JsonElement body, IMessageBus bus)
    {
        var instructions = body.Deserialize<List<BPay.BPayPaymentInstructionRequest>>(JsonOptions) ?? [];
        var response = await bus.InvokeAsync<BPay.ConvertBPayBatchResponse>(
            new BPay.ConvertBPayBatchCommand(instructions));
        return Results.Ok(response);
    }

    private static async Task<IResult> DispatchImtAsync(JsonElement body, IMessageBus bus)
    {
        var instructions = body.Deserialize<List<Imt.ImtPaymentInstructionRequest>>(JsonOptions) ?? [];
        var response = await bus.InvokeAsync<Imt.ConvertImtBatchResponse>(
            new Imt.ConvertImtBatchCommand(instructions));
        return Results.Ok(response);
    }

    private static async Task<IResult> DispatchPriorityPaymentAsync(JsonElement body, IMessageBus bus)
    {
        var instructions = body.Deserialize<List<PriorityPayments.PriorityPaymentInstructionRequest>>(JsonOptions) ?? [];
        var response = await bus.InvokeAsync<PriorityPayments.ConvertPriorityPaymentBatchResponse>(
            new PriorityPayments.ConvertPriorityPaymentBatchCommand(instructions));
        return Results.Ok(response);
    }

    private static async Task<IResult> DispatchFxAsync(JsonElement body, IMessageBus bus)
    {
        var instructions = body.Deserialize<List<Fx.FxPaymentInstructionRequest>>(JsonOptions) ?? [];
        var response = await bus.InvokeAsync<Fx.ConvertFxBatchResponse>(
            new Fx.ConvertFxBatchCommand(instructions));
        return Results.Ok(response);
    }

    private static PaymentRoutingResponse Rejected(string reason) =>
        new(false, null, [new PaymentRoutingError(HeaderErrorIndex, reason)]);

    private static PaymentRoutingResponse RejectedUnsupported(string?[] typeCodes)
    {
        var errors = new List<PaymentRoutingError>(typeCodes.Length);
        for (var index = 0; index < typeCodes.Length; index++)
        {
            errors.Add(new PaymentRoutingError(index, $"Unsupported payment type '{typeCodes[index]}'."));
        }

        return new PaymentRoutingResponse(false, null, errors);
    }

    private static string? GetPaymentTypeCode(JsonElement instruction)
    {
        if (instruction.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        foreach (var property in instruction.EnumerateObject())
        {
            if (string.Equals(property.Name, PaymentTypeCodeProperty, StringComparison.OrdinalIgnoreCase))
            {
                return property.Value.ValueKind == JsonValueKind.String ? property.Value.GetString() : null;
            }
        }

        return null;
    }
}
