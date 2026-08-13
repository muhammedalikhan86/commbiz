using CommBiz.Api.Features.DirectEntry;

namespace CommBiz.Api.Tests;

public class ConvertDirectEntryBatchHandlerTests
{
    private static PaymentInstructionRequest Instruction(string paymentType) =>
        new(
            PaymentType: paymentType,
            Bsb: "062-000",
            AccountNumber: "10001000",
            Indicator: "N",
            TransactionCode: "53",
            AmountInCents: 10050,
            AccountTitle: "CLIENT COMPANY XYZ",
            LodgementReference: "INVOICE 123456",
            TraceBsb: "063-000",
            TraceAccountNumber: "100000",
            RemitterName: "COMPANY ABCD P/L",
            WithholdingTaxAmountInCents: 0);

    private static ConvertDirectEntryBatchCommand CommandWith(
        IReadOnlyList<PaymentInstructionRequest> instructions) =>
        new(new ConvertDirectEntryBatchRequest(
            FileName: "COMPANY ABCD PTY LTD",
            UserIdentificationNumber: "301500",
            DescriptionOfEntries: "EFT-PAYMENT",
            DateToBeProcessed: new DateOnly(2026, 12, 5),
            Instructions: instructions));

    [Theory]
    [InlineData("DirectEntry")]
    [InlineData("directentry")]
    [InlineData("DIRECTENTRY")]
    public void All_supported_type_batch_proceeds_to_placeholder_conversion(string paymentType)
    {
        var command = CommandWith([Instruction(paymentType), Instruction(paymentType)]);

        var result = ConvertDirectEntryBatchHandler.Handle(command);

        Assert.True(result.Success);
        Assert.False(string.IsNullOrEmpty(result.ConvertedText));
        Assert.Null(result.Errors);
    }

    [Fact]
    public void Single_unsupported_type_instruction_rejects_the_whole_batch()
    {
        var command = CommandWith([Instruction("BPAY")]);

        var result = ConvertDirectEntryBatchHandler.Handle(command);

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
            Instruction("DirectEntry"),
            Instruction("BPAY"),
            Instruction("MT101")
        ]);

        var result = ConvertDirectEntryBatchHandler.Handle(command);

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
        var command = CommandWith([Instruction("DirectEntry"), Instruction("BPAY")]);

        var result = ConvertDirectEntryBatchHandler.Handle(command);

        Assert.False(result.Success);
        Assert.Null(result.ConvertedText);
    }

    [Fact]
    public void Empty_instructions_array_is_rejected_with_minimum_count_reason()
    {
        var command = CommandWith([]);

        var result = ConvertDirectEntryBatchHandler.Handle(command);

        Assert.False(result.Success);
        Assert.Null(result.ConvertedText);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Index == -1 && e.Reason.Contains("at least 2"));
    }

    [Fact]
    public void Valid_batch_converted_text_is_header_plus_the_mapped_detail_records_plus_trailer()
    {
        var first = Instruction("DirectEntry");
        var second = Instruction("DirectEntry");
        var command = CommandWith([first, second]);

        var result = ConvertDirectEntryBatchHandler.Handle(command);

        Assert.True(result.Success);
        var expected =
            DirectEntryHeaderRecordMapper.Map(command.Request) + "\r\n" +
            DirectEntryDetailRecordMapper.Map(first) + "\r\n" +
            DirectEntryDetailRecordMapper.Map(second) + "\r\n" +
            DirectEntryTrailerRecordMapper.Map(command.Request.Instructions) + "\r\n";
        Assert.Equal(expected, result.ConvertedText);
    }

    [Fact]
    public void Multi_instruction_batch_produces_one_crlf_terminated_detail_record_per_instruction_in_order()
    {
        var first = Instruction("DirectEntry") with { LodgementReference = "FIRST" };
        var second = Instruction("DirectEntry") with { LodgementReference = "SECOND" };
        var command = CommandWith([first, second]);

        var result = ConvertDirectEntryBatchHandler.Handle(command);

        Assert.True(result.Success);
        var expected =
            DirectEntryHeaderRecordMapper.Map(command.Request) + "\r\n" +
            DirectEntryDetailRecordMapper.Map(first) + "\r\n" +
            DirectEntryDetailRecordMapper.Map(second) + "\r\n" +
            DirectEntryTrailerRecordMapper.Map(command.Request.Instructions) + "\r\n";
        Assert.Equal(expected, result.ConvertedText);
    }
}
