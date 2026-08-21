using CommBiz.Api.Features.Fx;

namespace CommBiz.Api.Tests.Fx;

public class ConvertFxBatchHandlerTests
{
    private static readonly FxSettings Settings = new()
    {
        SellInstruction = "MAN",
        BuyInstruction = "DOC",
        BuyPaymentDetails = "Buy",
        SellPaymentDetails = "Sell",
    };

    // Confirmed sample shape, per the IPFX spec's Sample 2 ("New Settlement", USD/AUD, 500).
    private static FxPaymentInstructionRequest ValidInstruction() =>
        new(
            PaymentTypeCode: "FOREX",
            PaymentDate: DateTime.UtcNow.Date,
            Amount: 500.00m,
            Notes: "New Settlement",
            BuyCurrency: "USD",
            SellCurrency: "AUD",
            RateTypeCode: "SPOT",
            ValueDateTypeCode: "STANDARD",
            FeeTypeCode: "OUR",
            FeeOtherTypeCode: "",
            AccountNo: "Payment2");

    private static ConvertFxBatchCommand CommandWith(IReadOnlyList<FxPaymentInstructionRequest> instructions) =>
        new(instructions);

    [Fact]
    public void Valid_batch_returns_success_with_converted_text()
    {
        var command = CommandWith([ValidInstruction()]);

        var result = ConvertFxBatchHandler.Handle(command, Settings);

        Assert.True(result.Success);
        Assert.False(string.IsNullOrEmpty(result.ConvertedText));
        Assert.Null(result.Errors);
    }

    [Fact]
    public void Invalid_batch_returns_failure_with_per_instruction_reasons()
    {
        var command = CommandWith([ValidInstruction() with { AccountNo = "" }]);

        var result = ConvertFxBatchHandler.Handle(command, Settings);

        Assert.False(result.Success);
        Assert.Null(result.ConvertedText);
        Assert.NotNull(result.Errors);
        var error = Assert.Single(result.Errors);
        Assert.Equal(0, error.Index);
        Assert.Contains("AccountNo", error.Reason);
    }

    [Fact]
    public void Multi_instruction_batch_is_joined_with_CRLF_and_has_no_trailing_CRLF()
    {
        var first = ValidInstruction();
        var second = ValidInstruction() with { AccountNo = "Payment3" };
        var command = CommandWith([first, second]);

        var result = ConvertFxBatchHandler.Handle(command, Settings);

        Assert.True(result.Success);
        var convertedText = result.ConvertedText!;

        Assert.False(convertedText.EndsWith("\r\n", StringComparison.Ordinal));
        var rows = convertedText.Split("\r\n");
        Assert.Equal(2, rows.Length); // exactly one row per instruction - no header/trailer, no blank trailing row
        Assert.Equal(FxRecordMapper.Map(first, Settings), rows[0]);
        Assert.Equal(FxRecordMapper.Map(second, Settings), rows[1]);
    }

    [Fact]
    public void Single_instruction_batch_has_no_CRLF_at_all()
    {
        var command = CommandWith([ValidInstruction()]);

        var result = ConvertFxBatchHandler.Handle(command, Settings);

        Assert.DoesNotContain("\r\n", result.ConvertedText);
    }

    [Fact]
    public void Batch_exceeding_200_instructions_is_rejected_in_full()
    {
        var instructions = Enumerable.Repeat(ValidInstruction(), 201).ToArray();
        var command = CommandWith(instructions);

        var result = ConvertFxBatchHandler.Handle(command, Settings);

        Assert.False(result.Success);
        Assert.Null(result.ConvertedText);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Index == -1 && e.Reason.Contains("at most 200"));
    }

    [Fact]
    public void Two_instruction_batch_mappings_has_one_row_per_instruction_in_order_no_header_or_trailer()
    {
        var first = ValidInstruction();
        var second = ValidInstruction() with { AccountNo = "Payment3" };
        var command = CommandWith([first, second]);

        var result = ConvertFxBatchHandler.Handle(command, Settings);

        Assert.True(result.Success);
        Assert.Equal(["row1", "row2"], result.Mappings!.Select(line => line.Line));
    }

    [Fact]
    public void Row_mappings_have_exactly_27_fields_matching_the_same_instruction_used_to_build_that_row()
    {
        var first = ValidInstruction() with { AccountNo = "Payment2" };
        var second = ValidInstruction() with { AccountNo = "Payment3" };
        var command = CommandWith([first, second]);

        var result = ConvertFxBatchHandler.Handle(command, Settings);

        var row1 = result.Mappings!.Single(line => line.Line == "row1");
        var row2 = result.Mappings!.Single(line => line.Line == "row2");
        Assert.Equal(27, row1.Fields.Count);
        Assert.Equal(27, row2.Fields.Count);
        Assert.Equal("Payment2", row1.Fields.Single(f => f.CbaResponseField == "Transaction Description").RequestValue);
        Assert.Equal("Payment3", row2.Fields.Single(f => f.CbaResponseField == "Transaction Description").RequestValue);
    }

    [Fact]
    public void Mappings_is_null_when_validation_fails()
    {
        var command = CommandWith([ValidInstruction() with { AccountNo = "" }]);

        var result = ConvertFxBatchHandler.Handle(command, Settings);

        Assert.False(result.Success);
        Assert.Null(result.Mappings);
    }
}
