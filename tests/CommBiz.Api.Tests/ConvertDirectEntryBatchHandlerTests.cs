using CommBiz.Api.Features.DirectEntry;

namespace CommBiz.Api.Tests;

public class ConvertDirectEntryBatchHandlerTests
{
    private static readonly DirectEntrySettings Settings = new()
    {
        InstitutionCode = "CBA",
        UserIdentificationNumber = "301500",
        NameOfUserSupplyingFile = "SHAW AND PARTNERS LIMITED",
        Title = "SHAW AND PARTNERS LIMITED",
        DescriptionOfEntriesOnFile = "ONLINEPAYMENTS",
        LodgementReferenceDetails = "PAYMENTS",
        TraceAccountBsb = "062-000",
        TraceAccountAccNo = "21120227",
        NameOfRemitter = "SHAW AND PARTNER",
        AmountOfWithholdingTax = "00000000",
        TransactionCode = "13",
    };

    private static PaymentInstructionRequest Instruction(string paymentTypeCode) =>
        new(
            PaymentTypeCode: paymentTypeCode,
            AccountNo: "S1605677",
            PaymentSourceTypeCode: "CMA",
            SourceBankAccountName: "SOPHIA CLARK",
            SourceBankAccountNo: "111375004",
            SourceBankBsb: "015141",
            PaymentDate: new DateTime(2026, 8, 20, 10, 0, 0),
            SourceCurrency: "AUD",
            SourceAmount: 0.0m,
            Amount: 7500.0m,
            CreateBy: "James Harris");

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
    public void Single_unsupported_type_instruction_rejects_the_whole_batch()
    {
        var command = CommandWith([Instruction("BPAY"), Instruction("DE")]);

        var result = ConvertDirectEntryBatchHandler.Handle(command, Settings);

        Assert.False(result.Success);
        Assert.Null(result.ConvertedText);
        Assert.NotNull(result.Errors);
        var error = Assert.Single(result.Errors);
        Assert.Equal(0, error.Index);
        Assert.Contains("BPAY", error.Reason);
    }

    [Fact]
    public void Multiple_unsupported_type_instructions_are_all_reported()
    {
        var command = CommandWith(
        [
            Instruction("DE"),
            Instruction("BPAY"),
            Instruction("MT101")
        ]);

        var result = ConvertDirectEntryBatchHandler.Handle(command, Settings);

        Assert.False(result.Success);
        Assert.Null(result.ConvertedText);
        Assert.NotNull(result.Errors);
        Assert.Equal(2, result.Errors.Count);
        Assert.Equal(1, result.Errors[0].Index);
        Assert.Equal(2, result.Errors[1].Index);
    }

    [Fact]
    public void Mixed_valid_and_invalid_type_batch_is_rejected_in_full_with_no_partial_conversion()
    {
        var command = CommandWith([Instruction("DE"), Instruction("BPAY")]);

        var result = ConvertDirectEntryBatchHandler.Handle(command, Settings);

        Assert.False(result.Success);
        Assert.Null(result.ConvertedText);
    }

    [Fact]
    public void Empty_instructions_array_is_rejected_with_minimum_count_reason()
    {
        var command = CommandWith([]);

        var result = ConvertDirectEntryBatchHandler.Handle(command, Settings);

        Assert.False(result.Success);
        Assert.Null(result.ConvertedText);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Index == -1 && e.Reason.Contains("at least 2"));
    }

    [Fact]
    public void Valid_batch_converted_text_is_header_plus_the_mapped_detail_records_plus_trailer()
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
            DirectEntryTrailerRecordMapper.Map(command.Instructions, Settings) + "\r\n";
        Assert.Equal(expected, result.ConvertedText);
    }

    [Fact]
    public void Multi_instruction_batch_produces_one_crlf_terminated_detail_record_per_instruction_in_order()
    {
        var first = Instruction("DE") with { SourceBankAccountNo = "111375004" };
        var second = Instruction("DE") with { SourceBankAccountNo = "222486115" };
        var command = CommandWith([first, second]);

        var result = ConvertDirectEntryBatchHandler.Handle(command, Settings);

        Assert.True(result.Success);
        var expected =
            DirectEntryHeaderRecordMapper.Map(command.Instructions, Settings) + "\r\n" +
            DirectEntryDetailRecordMapper.Map(first, Settings) + "\r\n" +
            DirectEntryDetailRecordMapper.Map(second, Settings) + "\r\n" +
            DirectEntryTrailerRecordMapper.Map(command.Instructions, Settings) + "\r\n";
        Assert.Equal(expected, result.ConvertedText);
    }
}
