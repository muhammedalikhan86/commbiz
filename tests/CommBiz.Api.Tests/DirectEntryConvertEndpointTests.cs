using System.Net;
using System.Net.Http.Json;
using CommBiz.Api.Features.DirectEntry;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace CommBiz.Api.Tests;

public class DirectEntryConvertEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    // Confirmed static Direct Entry config values, set explicitly in the test host so this test
    // doesn't depend on appsettings.json's contents.
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

    private static PaymentInstructionRequest ValidInstruction(string accountNo = "S1605677") =>
        new(
            PaymentTypeCode: "DE",
            AccountNo: accountNo,
            PaymentSourceTypeCode: "CMA",
            SourceBankAccountName: "SOPHIA CLARK",
            SourceBankAccountNo: "111375004",
            SourceBankBsb: "015141",
            PaymentDate: new DateTime(2026, 8, 20, 10, 0, 0),
            SourceCurrency: "AUD",
            SourceAmount: 0.0m,
            Amount: 7500.0m,
            CreateBy: "James Harris");

    private static List<PaymentInstructionRequest> WellFormedBatch(
        IReadOnlyList<PaymentInstructionRequest>? instructions = null) =>
        // F-008: the spec requires at least 2 detail records, so the default batch has 2.
        [.. instructions ?? [ValidInstruction("S1605677"), ValidInstruction("S1605678")]];

    [Fact]
    public async Task Well_formed_batch_is_dispatched_through_wolverine_and_returns_success_with_converted_text()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/convert", WellFormedBatch());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ConvertDirectEntryBatchResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.False(string.IsNullOrEmpty(result.ConvertedText));
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task Empty_instructions_array_is_rejected_with_minimum_count_reason()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/convert", WellFormedBatch(instructions: []));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ConvertDirectEntryBatchResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Null(result.ConvertedText);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Index == -1 && e.Reason.Contains("at least 2"));
    }

    [Fact]
    public async Task Valid_three_instruction_batch_produces_a_structurally_correct_assembled_file()
    {
        var client = CreateClient();
        var request = WellFormedBatch(
        [
            ValidInstruction("S1605677"),
            ValidInstruction("S1605678"),
            ValidInstruction("S1605679")
        ]);

        var response = await client.PostAsJsonAsync("/convert", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ConvertDirectEntryBatchResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.ConvertedText);

        // ConvertedText is CRLF-terminated per line, including the trailer, so splitting on "\r\n"
        // leaves one trailing empty entry that isn't a record.
        var lines = result.ConvertedText.Split("\r\n");
        Assert.Equal(string.Empty, lines[^1]);
        var recordLines = lines[..^1];

        Assert.Equal(5, recordLines.Length); // 1 header + 3 details + 1 trailer
        Assert.All(recordLines, line => Assert.Equal(120, line.Length));
        Assert.Equal(['0', '1', '1', '1', '7'], recordLines.Select(line => line[0]));
    }
}
