using System.Net;
using System.Net.Http.Json;
using CommBiz.Api.Features.Fx;
using CommBiz.Api.Features.PaymentRouting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace CommBiz.Api.Tests.Fx;

public class FxConvertEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    // Confirmed-shape Fx config, set explicitly in the test host so this test doesn't
    // depend on appsettings.json's contents (mirrors PriorityPaymentConvertEndpointTests' approach).
    private static readonly Dictionary<string, string?> FxConfig = new()
    {
        ["Fx:SellInstruction"] = "MAN",
        ["Fx:BuyInstruction"] = "DOC",
        ["Fx:BuyPaymentDetails"] = "Buy",
        ["Fx:SellPaymentDetails"] = "Sell",
    };

    private HttpClient CreateClient() =>
        factory.WithWebHostBuilder(builder =>
                builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(FxConfig)))
            .CreateClient();

    // Confirmed sample shape, per the IPFX spec's Sample 2 ("New Settlement", USD/AUD, 500).
    private static object ValidInstruction(string buyCurrency = "USD", string accountNo = "Payment2") =>
        new
        {
            PaymentTypeCode = "FOREX",
            Amount = 500.00m,
            BuyCurrency = buyCurrency,
            SellCurrency = "AUD",
            AccountNo = accountNo,
        };

    [Fact]
    public async Task Valid_FOREX_batch_is_dispatched_through_the_router_to_the_fx_handler_and_returns_success()
    {
        var client = CreateClient();
        var batch = new[] { ValidInstruction() };

        var response = await client.PostAsJsonAsync("/convert", batch);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ConvertFxBatchResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.False(string.IsNullOrEmpty(result.ConvertedText));
        Assert.Null(result.Errors);
        Assert.StartsWith("FX,", result.ConvertedText, StringComparison.Ordinal);
        Assert.NotNull(result.Mappings);
        Assert.Single(result.Mappings);
        Assert.Equal("row1", result.Mappings[0].Line);
    }

    [Fact]
    public async Task Invalid_FOREX_batch_returns_failure_with_per_instruction_reasons_and_null_mappings()
    {
        var client = CreateClient();
        var batch = new[] { ValidInstruction(buyCurrency: "US") };

        var response = await client.PostAsJsonAsync("/convert", batch);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ConvertFxBatchResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Null(result.ConvertedText);
        Assert.Null(result.Mappings);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Index == 0 && e.Reason.Contains("BuyCurrency"));
    }

    [Fact]
    public async Task Batch_mixing_FOREX_with_another_payment_type_is_rejected_in_full()
    {
        var client = CreateClient();
        var batch = new object[]
        {
            ValidInstruction(),
            new { PaymentTypeCode = "DE" },
        };

        var response = await client.PostAsJsonAsync("/convert", batch);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaymentRoutingResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Null(result.ConvertedText);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Reason.Contains("must not mix payment types"));
    }
}
