using System.Net;
using System.Net.Http.Json;
using CommBiz.Api.Features.DirectEntry;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CommBiz.Api.Tests;

public class DirectEntryConvertEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private static PaymentInstructionRequest ValidInstruction(string lodgementReference = "INVOICE 123456") =>
        new(
            PaymentType: "DirectEntry",
            Bsb: "062-000",
            AccountNumber: "10001000",
            Indicator: "N",
            TransactionCode: "53",
            AmountInCents: 10050,
            AccountTitle: "CLIENT COMPANY XYZ",
            LodgementReference: lodgementReference,
            TraceBsb: "063-000",
            TraceAccountNumber: "100000",
            RemitterName: "COMPANY ABCD P/L",
            WithholdingTaxAmountInCents: 0);

    private static ConvertDirectEntryBatchRequest WellFormedRequest(
        IReadOnlyList<PaymentInstructionRequest>? instructions = null) =>
        new(
            FileName: "COMPANY ABCD PTY LTD",
            UserIdentificationNumber: "301500",
            DescriptionOfEntries: "EFT-PAYMENT",
            DateToBeProcessed: new DateOnly(2026, 12, 5),
            // F-008: the spec requires at least 2 detail records, so the default batch has 2.
            Instructions: instructions ?? [ValidInstruction("INVOICE 123456"), ValidInstruction("INVOICE 123457")]);

    [Fact]
    public async Task Well_formed_batch_is_dispatched_through_wolverine_and_returns_success_with_converted_text()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/direct-entry/convert", WellFormedRequest());

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
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/direct-entry/convert",
            WellFormedRequest(instructions: []));

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
        var client = factory.CreateClient();
        var request = WellFormedRequest(
        [
            ValidInstruction("INVOICE 123456"),
            ValidInstruction("INVOICE 123457"),
            ValidInstruction("INVOICE 123458")
        ]);

        var response = await client.PostAsJsonAsync("/direct-entry/convert", request);

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
