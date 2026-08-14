using System.Net;
using System.Net.Http.Json;
using CommBiz.Api.Features.Imt;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace CommBiz.Api.Tests.Imt;

public class ImtConvertEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    // Confirmed-shape Imt config, set explicitly in the test host so this test doesn't depend on
    // appsettings.json's contents (mirrors BPayConvertEndpointTests' approach).
    private static readonly Dictionary<string, string?> ImtConfig = new()
    {
        ["Imt:DebitAccountBsb"] = "062-000",
        ["Imt:DebitAccountNumber"] = "2112 0075",
        ["Imt:DebitAccountName"] = "SHAW - AUD TRUST ACCOUNT",
    };

    private HttpClient CreateClient() =>
        factory.WithWebHostBuilder(builder =>
                builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(ImtConfig)))
            .CreateClient();

    // Confirmed real sample request shape from the Task Packet.
    private static object ValidInstruction(string paymentReference = "TT-000001") =>
        new
        {
            PaymentTypeCode = "TT",
            PaymentSourceTypeCode = "LEDGER",
            SourceBankAccountName = (string?)null,
            SourceBankAccountNo = (string?)null,
            SourceBankBSB = (string?)null,
            DestinationBankTypeCode = (string?)null,
            DestinationBankAccountName = "SAMER MOHAMMED KIKI",
            DestinationBankAccountNo = "658450191",
            PaymentDate = DateTime.UtcNow.Date.AddDays(1),
            SourceCurrency = "USD",
            SourceAmount = 588517.58m,
            Amount = 0.0m,
            PaymentReference = paymentReference,
            Notes = "10 bps on fx",
            Currency = (string?)null,
            DestinationBankIBAN = (string?)null,
            DestinationBankSWIFTCode = "CHASUS33",
            DestinationBankName = "NATIONAL FINANCIAL SERVICES",
            DestinationBankAddress = "640 5TH AVENUE NEW YORK NY 10019",
            BeneficiaryAddress = "9101 Alta Drive, U15, Las Vegas, NV 89145",
            IntermediaryBankIBAN = (string?)null,
            IntermediaryBankSWIFTCode = "CHASUS33",
            IntermediaryBankName = "CHASE MANHATTAN BANK",
            IntermediaryBankAddress = "1 CHASE MANHATTAN PLAZA NEW YORK NY 10005",
        };

    [Fact]
    public async Task Valid_TT_batch_is_dispatched_through_the_router_to_the_IMT_handler_and_returns_success()
    {
        var client = CreateClient();
        var batch = new[] { ValidInstruction() };

        var response = await client.PostAsJsonAsync("/convert", batch);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ConvertImtBatchResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.False(string.IsNullOrEmpty(result.ConvertedText));
        Assert.Null(result.Errors);
        Assert.StartsWith("IMT,", result.ConvertedText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Invalid_TT_batch_returns_failure_with_per_instruction_reasons()
    {
        var client = CreateClient();
        var batch = new[] { ValidInstruction(paymentReference: "") };

        var response = await client.PostAsJsonAsync("/convert", batch);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ConvertImtBatchResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Null(result.ConvertedText);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Index == 0 && e.Reason.Contains("PaymentReference"));
    }

    [Fact]
    public async Task Bare_IMT_payment_type_code_is_not_a_real_API_code_and_is_rejected_as_unsupported()
    {
        var client = CreateClient();
        var batch = new[] { new { paymentTypeCode = "IMT" } };

        var response = await client.PostAsJsonAsync("/convert", batch);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CommBiz.Api.Features.PaymentRouting.PaymentRoutingResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Reason.Contains("Unsupported"));
    }
}
