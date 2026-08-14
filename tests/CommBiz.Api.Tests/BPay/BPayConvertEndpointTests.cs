using System.Net;
using System.Net.Http.Json;
using CommBiz.Api.Features.BPay;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace CommBiz.Api.Tests.BPay;

public class BPayConvertEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    // Confirmed-shape placeholder BPay config, set explicitly in the test host so this test doesn't
    // depend on appsettings.json's contents (mirrors DirectEntryConvertEndpointTests' approach).
    private static readonly Dictionary<string, string?> BPayConfig = new()
    {
        ["BPay:FundingAccount"] = "06200012345678",
        ["BPay:FileNumber"] = "001",
    };

    private HttpClient CreateClient() =>
        factory.WithWebHostBuilder(builder =>
                builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(BPayConfig)))
            .CreateClient();

    private static object ValidInstruction(string billerCode = "488577", string reference = "1202194308172118") =>
        new
        {
            PaymentTypeCode = "BPAY",
            AccountNo = "S1218937",
            PaymentSourceTypeCode = "LEDGER",
            PaymentDate = new DateTime(2026, 8, 11, 10, 0, 0),
            Amount = 10000.00m,
            BPayBillerCode = billerCode,
            BPayReference = reference,
        };

    [Fact]
    public async Task Valid_BPAY_batch_is_dispatched_through_the_router_and_returns_success_with_converted_text()
    {
        var client = CreateClient();
        var batch = new[] { ValidInstruction(), ValidInstruction() };

        var response = await client.PostAsJsonAsync("/convert", batch);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ConvertBPayBatchResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.False(string.IsNullOrEmpty(result.ConvertedText));
        Assert.Null(result.Errors);

        var lines = result.ConvertedText!.Split("\r\n");
        Assert.Equal(string.Empty, lines[^1]);
        var recordLines = lines[..^1];
        Assert.Equal(3, recordLines.Length); // 1 header + 2 details, no trailer
        Assert.Equal(['0', '5', '5'], recordLines.Select(line => line[0]));
    }

    [Fact]
    public async Task Invalid_BPAY_batch_returns_failure_with_per_instruction_reasons()
    {
        var client = CreateClient();
        var batch = new[] { ValidInstruction(billerCode: "NOTNUMERIC"), ValidInstruction(reference: "") };

        var response = await client.PostAsJsonAsync("/convert", batch);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ConvertBPayBatchResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Null(result.ConvertedText);
        Assert.NotNull(result.Errors);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Index == 0 && e.Reason.Contains("BPayBillerCode"));
        Assert.Contains(result.Errors, e => e.Index == 1 && e.Reason.Contains("BPayReference"));
    }

    [Fact]
    public async Task Batch_exceeding_200_instructions_is_rejected_with_max_count_reason()
    {
        var client = CreateClient();
        var batch = Enumerable.Repeat(ValidInstruction(), 201).ToArray();

        var response = await client.PostAsJsonAsync("/convert", batch);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ConvertBPayBatchResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Null(result.ConvertedText);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Index == -1 && e.Reason.Contains("at most 200"));
    }
}
