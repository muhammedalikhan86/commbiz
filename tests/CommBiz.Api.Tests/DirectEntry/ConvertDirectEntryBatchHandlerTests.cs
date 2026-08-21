using CommBiz.Api.Features.DirectEntry;

namespace CommBiz.Api.Tests.DirectEntry;

public class ConvertDirectEntryBatchHandlerTests
{
    private static readonly DirectEntrySettings Settings = new()
    {
        DescriptionOfEntriesOnFile = "ONLINEPAYMENTS",
        LodgementReferenceDetails = "PAYMENTS",
        TraceAccountBsb = "062-000",
        TraceAccountAccNo = "21120227",
        NameOfRemitter = "SHAW AND PARTNER",
        AmountOfWithholdingTax = "00000000"
    };

    private static PaymentInstructionRequest Instruction(string paymentTypeCode) =>
        new(
            PaymentTypeCode: paymentTypeCode,
            DestinationBankBsb: "484799",
            DestinationBankAccountNo: "300500",
            DestinationBankAccountName: "JOHN CITIZEN",
            PaymentDate: new DateTime(2026, 8, 20, 10, 0, 0),
            Amount: 7500.0m);

    private static ConvertDirectEntryBatchCommand CommandWith(
        IReadOnlyList<PaymentInstructionRequest> instructions) => new(instructions);

    [Theory]
    [InlineData("DE")]
    [InlineData("de")]
    [InlineData("De")]
    public void All_supported_type_batch_proceeds_to_conversion(string paymentTypeCode)
    {
        var command = CommandWith([Instruction(paymentTypeCode), Instruction(paymentTypeCode)]);

        var result = ConvertDirectEntryBatchHandler.Handle(command, Settings);

        Assert.True(result.Success);
        Assert.False(string.IsNullOrEmpty(result.ConvertedText));
        Assert.Null(result.Errors);
    }

    [Fact]
    public void Empty_instructions_array_is_rejected_with_minimum_count_reason()
    {
        var command = CommandWith([]);

        var result = ConvertDirectEntryBatchHandler.Handle(command, Settings);

        Assert.False(result.Success);
        Assert.Null(result.ConvertedText);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Index == -1 && e.Reason.Contains("at least 1"));
    }

    [Fact]
    public void Single_instruction_batch_is_accepted_as_the_new_minimum()
    {
        var command = CommandWith([Instruction("DE")]);

        var result = ConvertDirectEntryBatchHandler.Handle(command, Settings);

        Assert.True(result.Success);
        Assert.False(string.IsNullOrEmpty(result.ConvertedText));
        Assert.Null(result.Errors);
    }

    [Fact]
    public void Valid_batch_converted_text_is_header_plus_the_mapped_detail_records_plus_self_balancing_plus_trailer()
    {
        var first = Instruction("DE");
        var second = Instruction("DE");
        var command = CommandWith([first, second]);

        var result = ConvertDirectEntryBatchHandler.Handle(command, Settings);

        Assert.True(result.Success);
        var expected =
            DirectEntryHeaderRecordMapper.Map(command.Instructions, Settings) + "\r\n" +
            DirectEntryDetailRecordMapper.Map(first, Settings) + "\r\n" +
            DirectEntryDetailRecordMapper.Map(second, Settings) + "\r\n" +
            DirectEntrySelfBalancingRecordMapper.Map(command.Instructions, Settings) + "\r\n" +
            DirectEntryTrailerRecordMapper.Map(command.Instructions, Settings) + "\r\n";
        Assert.Equal(expected, result.ConvertedText);
    }

    [Fact]
    public void Multi_instruction_batch_produces_one_crlf_terminated_detail_record_per_instruction_in_order()
    {
        var first = Instruction("DE");
        var second = Instruction("DE");
        var command = CommandWith([first, second]);

        var result = ConvertDirectEntryBatchHandler.Handle(command, Settings);

        Assert.True(result.Success);
        var expected =
            DirectEntryHeaderRecordMapper.Map(command.Instructions, Settings) + "\r\n" +
            DirectEntryDetailRecordMapper.Map(first, Settings) + "\r\n" +
            DirectEntryDetailRecordMapper.Map(second, Settings) + "\r\n" +
            DirectEntrySelfBalancingRecordMapper.Map(command.Instructions, Settings) + "\r\n" +
            DirectEntryTrailerRecordMapper.Map(command.Instructions, Settings) + "\r\n";
        Assert.Equal(expected, result.ConvertedText);
    }

    [Fact]
    public void Self_balancing_record_is_positioned_immediately_before_the_trailer_after_all_real_details()
    {
        var command = CommandWith([Instruction("DE"), Instruction("DE"), Instruction("DE")]);

        var result = ConvertDirectEntryBatchHandler.Handle(command, Settings);

        Assert.True(result.Success);
        var lines = result.ConvertedText!.Split("\r\n");
        var recordLines = lines[..^1]; // trailing empty entry from the final CRLF, not a record

        Assert.Equal(6, recordLines.Length); // 1 header + 3 details + 1 self-balancing + 1 trailer
        Assert.Equal("0", recordLines[0][0..1]); // header
        Assert.Equal("1", recordLines[1][0..1]); // detail
        Assert.Equal("1", recordLines[2][0..1]); // detail
        Assert.Equal("1", recordLines[3][0..1]); // detail
        Assert.Equal("1", recordLines[4][0..1]); // self-balancing
        Assert.Equal("7", recordLines[5][0..1]); // trailer
    }

    [Fact]
    public void Two_instruction_batch_mappings_has_one_line_per_detail_plus_header_selfbalancing_trailer_in_order()
    {
        var command = CommandWith([Instruction("DE"), Instruction("DE")]);

        var result = ConvertDirectEntryBatchHandler.Handle(command, Settings);

        Assert.True(result.Success);
        Assert.NotNull(result.Mappings);
        Assert.Equal(
            ["header", "detail1", "detail2", "selfbalancing", "trailer"],
            result.Mappings!.Select(line => line.Line));
    }

    [Fact]
    public void Two_instruction_batch_mappings_detail_fields_match_the_converted_text_for_that_instruction()
    {
        var first = Instruction("DE") with { DestinationBankAccountNo = "111375004" };
        var second = Instruction("DE") with { DestinationBankAccountNo = "222486115" };
        var command = CommandWith([first, second]);

        var result = ConvertDirectEntryBatchHandler.Handle(command, Settings);

        var detail1 = result.Mappings!.Single(line => line.Line == "detail1");
        var detail2 = result.Mappings!.Single(line => line.Line == "detail2");
        Assert.Equal("111375004", detail1.Fields.Single(f => f.CbaResponseField == "Account Number to be Credited/Debited").RequestValue);
        Assert.Equal("222486115", detail2.Fields.Single(f => f.CbaResponseField == "Account Number to be Credited/Debited").RequestValue);
    }

    [Fact]
    public void Single_instruction_batch_mappings_has_only_detail1_no_detail2()
    {
        var command = CommandWith([Instruction("DE")]);

        var result = ConvertDirectEntryBatchHandler.Handle(command, Settings);

        Assert.True(result.Success);
        Assert.Equal(
            ["header", "detail1", "selfbalancing", "trailer"],
            result.Mappings!.Select(line => line.Line));
    }

    [Fact]
    public void Mappings_is_null_when_validation_fails()
    {
        var command = CommandWith([]);

        var result = ConvertDirectEntryBatchHandler.Handle(command, Settings);

        Assert.False(result.Success);
        Assert.Null(result.Mappings);
    }
}
