using System.Net;
using System.Net.Http.Json;
using CommBiz.Api.Features.PriorityPayments;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace CommBiz.Api.Tests.PriorityPayments;

public class PriorityPaymentConvertEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    // Confirmed-shape PriorityPayments config, set explicitly in the test host so this test doesn't
    // depend on appsettings.json's contents (mirrors ImtConvertEndpointTests' approach).
    private static readonly Dictionary<string, string?> PriorityPaymentsConfig = new()
    {
        ["PriorityPayments:DebitAccountBsb"] = "062-000",
        ["PriorityPayments:DebitAccountNumber"] = "2112 0075",
        ["PriorityPayments:DebitAccountName"] = "SHAW - AUD TRUST ACCOUNT",
    };

    private HttpClient CreateClient() =>
        factory.WithWebHostBuilder(builder =>
                builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(PriorityPaymentsConfig)))
            .CreateClient();

    // Confirmed real sample request shape from the Task Packet.
    private static object ValidInstruction(string notes = "Accounts has been paid to before.") =>
        new
        {
            PaymentTypeCode = "RTGS",
            DestinationBankAccountName = "ORS APP GATB",
            DestinationBankAccountNo = "838629371",
            DestinationBankBSB = "012110",
            PaymentDate = DateTime.UtcNow.Date.AddDays(1),
            Amount = 10775.0m,
            Notes = notes,
            BeneficiaryAddress = (string?)null,
        };

    [Fact]
    public async Task Valid_RTGS_batch_is_dispatched_through_the_router_to_the_priority_payments_handler_and_returns_success()
    {
        var client = CreateClient();
        var batch = new[] { ValidInstruction() };

        var response = await client.PostAsJsonAsync("/convert", batch);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ConvertPriorityPaymentBatchResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.False(string.IsNullOrEmpty(result.ConvertedText));
        Assert.Null(result.Errors);
        Assert.StartsWith("PP,", result.ConvertedText, StringComparison.Ordinal);
        Assert.NotNull(result.Mappings);
        Assert.Single(result.Mappings);
        Assert.Equal("row1", result.Mappings[0].Line);
    }

    [Fact]
    public async Task Invalid_RTGS_batch_returns_failure_with_per_instruction_reasons_and_null_mappings()
    {
        var client = CreateClient();
        var batch = new[] { ValidInstruction(notes: "") };

        var response = await client.PostAsJsonAsync("/convert", batch);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ConvertPriorityPaymentBatchResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Null(result.ConvertedText);
        Assert.Null(result.Mappings);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Index == 0 && e.Reason.Contains("Notes"));
    }
}
