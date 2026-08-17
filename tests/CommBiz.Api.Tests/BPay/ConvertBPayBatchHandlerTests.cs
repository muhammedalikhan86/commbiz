using CommBiz.Api.Features.BPay;

namespace CommBiz.Api.Tests.BPay;

public class ConvertBPayBatchHandlerTests
{
    private static readonly BPaySettings Settings = new()
    {
        FundingAccount = "06200012345678",
        FileNumber = "001",
    };

    private static BPayPaymentInstructionRequest ValidInstruction() =>
        new(
            PaymentTypeCode: "BPAY",
            AccountNo: "S1218937",
            PaymentSourceTypeCode: "LEDGER",
            PaymentDate: new DateTime(2026, 8, 11, 10, 0, 0),
            Amount: 10000.00m,
            BPayBillerCode: "488577",
            BPayReference: "1202194308172118");

    private static ConvertBPayBatchCommand CommandWith(
        IReadOnlyList<BPayPaymentInstructionRequest> instructions) => new(instructions);

    [Fact]
    public void Valid_batch_returns_success_with_converted_text()
    {
        var command = CommandWith([ValidInstruction()]);

        var result = ConvertBPayBatchHandler.Handle(command, Settings);

        Assert.True(result.Success);
        Assert.False(string.IsNullOrEmpty(result.ConvertedText));
        Assert.Null(result.Errors);
    }

    [Fact]
    public void Invalid_batch_returns_failure_with_per_instruction_reasons()
    {
        var command = CommandWith([ValidInstruction() with { BPayBillerCode = "" }]);

        var result = ConvertBPayBatchHandler.Handle(command, Settings);

        Assert.False(result.Success);
        Assert.Null(result.ConvertedText);
        Assert.NotNull(result.Errors);
        var error = Assert.Single(result.Errors);
        Assert.Equal(0, error.Index);
        Assert.Contains("BPayBillerCode", error.Reason);
    }

    [Fact]
    public void Empty_instructions_array_is_rejected_with_minimum_count_reason()
    {
        var command = CommandWith([]);

        var result = ConvertBPayBatchHandler.Handle(command, Settings);

        Assert.False(result.Success);
        Assert.Null(result.ConvertedText);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Index == -1 && e.Reason.Contains("at least 1"));
    }

    [Fact]
    public void Batch_exceeding_200_instructions_is_rejected_in_full()
    {
        var instructions = Enumerable.Repeat(ValidInstruction(), 201).ToArray();
        var command = CommandWith(instructions);

        var result = ConvertBPayBatchHandler.Handle(command, Settings);

        Assert.False(result.Success);
        Assert.Null(result.ConvertedText);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Index == -1 && e.Reason.Contains("at most 200"));
    }

    [Fact]
    public void Exactly_200_instructions_is_accepted()
    {
        var instructions = Enumerable.Repeat(ValidInstruction(), 200).ToArray();
        var command = CommandWith(instructions);

        var result = ConvertBPayBatchHandler.Handle(command, Settings);

        Assert.True(result.Success);
        Assert.False(string.IsNullOrEmpty(result.ConvertedText));
        Assert.Null(result.Errors);
    }

    [Fact]
    public void Valid_batch_converted_text_is_header_plus_the_mapped_detail_records_only_no_trailer()
    {
        var first = ValidInstruction();
        var second = ValidInstruction();
        var command = CommandWith([first, second]);

        var result = ConvertBPayBatchHandler.Handle(command, Settings);

        Assert.True(result.Success);
        var lines = result.ConvertedText!.Split("\r\n");
        Assert.Equal(string.Empty, lines[^1]);
        var recordLines = lines[..^1];

        Assert.Equal(3, recordLines.Length); // 1 header + 2 details, no trailer
        Assert.Equal(['0', '5', '5'], recordLines.Select(line => line[0]));
        Assert.Equal(BPayDetailRecordMapper.Map(first), recordLines[1]);
        Assert.Equal(BPayDetailRecordMapper.Map(second), recordLines[2]);
    }

    [Fact]
    public void Two_instruction_batch_mappings_has_one_line_per_detail_plus_header_in_order()
    {
        var command = CommandWith([ValidInstruction(), ValidInstruction()]);

        var result = ConvertBPayBatchHandler.Handle(command, Settings);

        Assert.True(result.Success);
        Assert.Equal(["header", "detail1", "detail2"], result.Mappings!.Select(line => line.Line));
    }

    [Fact]
    public void Mappings_header_line_attributes_funding_account_and_file_number_to_the_static_config_field_name()
    {
        var command = CommandWith([ValidInstruction()]);

        var result = ConvertBPayBatchHandler.Handle(command, Settings);

        var header = result.Mappings!.Single(line => line.Line == "header");
        Assert.Equal(nameof(BPaySettings.FundingAccount), header.Fields.Single(f => f.CbaResponseField == "Payment Account").RequestField);
        Assert.Equal(nameof(BPaySettings.FileNumber), header.Fields.Single(f => f.CbaResponseField == "File Number").RequestField);
    }

    [Fact]
    public void Mappings_is_null_when_validation_fails()
    {
        var command = CommandWith([ValidInstruction() with { BPayBillerCode = "" }]);

        var result = ConvertBPayBatchHandler.Handle(command, Settings);

        Assert.False(result.Success);
        Assert.Null(result.Mappings);
    }

    // F-021 fix: guards against ConvertedText and Mappings header line being built from independently
    // resolved DateTime.UtcNow calls, which could disagree by a second across a clock-second boundary.
    [Fact]
    public void Mappings_header_file_creation_date_and_time_match_the_values_written_into_converted_text()
    {
        var command = CommandWith([ValidInstruction()]);

        var result = ConvertBPayBatchHandler.Handle(command, Settings);

        var headerLine = result.ConvertedText!.Split("\r\n")[0].Split(",");
        var header = result.Mappings!.Single(line => line.Line == "header");
        Assert.Equal(headerLine[1], header.Fields.Single(f => f.CbaResponseField == "File Creation Date").CbaResponseValue);
        Assert.Equal(headerLine[2], header.Fields.Single(f => f.CbaResponseField == "File Creation Time").CbaResponseValue);
    }
}
