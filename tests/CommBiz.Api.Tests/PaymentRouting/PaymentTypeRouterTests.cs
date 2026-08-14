using System.Net;
using System.Net.Http.Json;
using CommBiz.Api.Features.PaymentRouting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace CommBiz.Api.Tests.PaymentRouting;

public class PaymentTypeRouterTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    // Same confirmed static Direct Entry config values as DirectEntryConvertEndpointTests, needed
    // whenever a batch dispatches through to the DE slice.
    private static readonly Dictionary<string, string?> DirectEntryConfig = new()
    {
        ["DirectEntry:InstitutionCode"] = "CBA",
        ["DirectEntry:UserIdentificationNumber"] = "301500",
        ["DirectEntry:NameOfUserSupplyingFile"] = "SHAW AND PARTNERS LIMITED",
        ["DirectEntry:Title"] = "SHAW AND PARTNERS LIMITED",
        ["DirectEntry:DescriptionOfEntriesOnFile"] = "ONLINEPAYMENTS",
        ["DirectEntry:LodgementReferenceDetails"] = "PAYMENTS",
        ["DirectEntry:TraceAccountBsb"] = "062-000",
        ["DirectEntry:TraceAccountAccNo"] = "21120227",
        ["DirectEntry:NameOfRemitter"] = "SHAW AND PARTNER",
        ["DirectEntry:AmountOfWithholdingTax"] = "00000000",
        ["DirectEntry:TransactionCode"] = "13",
    };

    private HttpClient CreateClient() =>
        factory.WithWebHostBuilder(builder =>
                builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(DirectEntryConfig)))
            .CreateClient();

    private static object ValidDirectEntryInstruction(string paymentTypeCode = "DE", string accountNo = "S1605677") =>
        new
        {
            PaymentTypeCode = paymentTypeCode,
            AccountNo = accountNo,
            PaymentSourceTypeCode = "CMA",
            SourceBankAccountName = "SOPHIA CLARK",
            SourceBankAccountNo = "111375004",
            SourceBankBsb = "015141",
            PaymentDate = new DateTime(2026, 8, 20, 10, 0, 0),
            SourceCurrency = "AUD",
            SourceAmount = 0.0m,
            Amount = 7500.0m,
            CreateBy = "James Harris",
        };

    [Fact]
    public async Task Empty_batch_is_rejected_with_no_type_to_route_on()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/convert", Array.Empty<object>());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaymentRoutingResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Null(result.ConvertedText);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Index == -1 && e.Reason.Contains("at least 1"));
    }

    [Fact]
    public async Task Batch_mixing_payment_types_is_rejected_in_full()
    {
        var client = CreateClient();
        var batch = new object[] { ValidDirectEntryInstruction("DE"), new { PaymentTypeCode = "BPAY" } };

        var response = await client.PostAsJsonAsync("/convert", batch);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaymentRoutingResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Null(result.ConvertedText);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Reason.Contains("must not mix"));
    }

    [Theory]
    [InlineData("IMT")]
    [InlineData("PP")]
    public async Task Batch_declaring_an_unsupported_type_is_rejected_in_full(string unsupportedType)
    {
        var client = CreateClient();
        var batch = new object[] { new { PaymentTypeCode = unsupportedType }, new { PaymentTypeCode = unsupportedType } };

        var response = await client.PostAsJsonAsync("/convert", batch);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaymentRoutingResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Null(result.ConvertedText);
        Assert.NotNull(result.Errors);
        Assert.Equal(2, result.Errors.Count);
        Assert.All(result.Errors, e => Assert.Contains($"Unsupported payment type '{unsupportedType}'", e.Reason));
    }

    [Theory]
    [InlineData("DE")]
    [InlineData("de")]
    [InlineData("De")]
    public async Task A_de_batch_still_dispatches_to_the_direct_entry_slice_case_insensitively(string paymentTypeCode)
    {
        var client = CreateClient();
        var batch = new object[] { ValidDirectEntryInstruction(paymentTypeCode) };

        var response = await client.PostAsJsonAsync("/convert", batch);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Features.DirectEntry.ConvertDirectEntryBatchResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.False(string.IsNullOrEmpty(result.ConvertedText));
        Assert.Null(result.Errors);
    }

    [Theory]
    [InlineData("BPAY")]
    [InlineData("bpay")]
    public async Task A_bpay_batch_dispatches_to_the_bpay_slice_case_insensitively(string paymentTypeCode)
    {
        var client = CreateClient();
        var batch = new object[] { new { PaymentTypeCode = paymentTypeCode } };

        var response = await client.PostAsJsonAsync("/convert", batch);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Features.BPay.ConvertBPayBatchResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
    }
}
