using CommBiz.Api.Features.Imt;

namespace CommBiz.Api.Tests.Imt;

public class ConvertImtBatchHandlerTests
{
    private static readonly ImtSettings Settings = new()
    {
        DebitAccountBsb = "062-000",
        DebitAccountNumber = "2112 0075",
        DebitAccountName = "SHAW - AUD TRUST ACCOUNT",
    };

    private static ImtPaymentInstructionRequest ValidInstruction() =>
        new(
            PaymentTypeCode: "TT",
            DestinationBankAccountName: "SAMER MOHAMMED KIKI",
            DestinationBankAccountNo: "658450191",
            PaymentDate: DateTime.UtcNow.Date.AddDays(1),
            SourceCurrency: "USD",
            SourceAmount: 588517.58m,
            Amount: 0.0m,
            PaymentReference: "TT-000001",
            Notes: "10 bps on fx",
            DestinationBankSwiftCode: "CHASUS33",
            DestinationBankName: "NATIONAL FINANCIAL SERVICES",
            BeneficiaryAddress: "9101 Alta Drive, U15, Las Vegas, NV 89145",
            IntermediaryBankSwiftCode: "CHASUS33",
            IntermediaryBankName: "CHASE MANHATTAN BANK");

    private static ConvertImtBatchCommand CommandWith(IReadOnlyList<ImtPaymentInstructionRequest> instructions) => new(instructions);

    [Fact]
    public void Valid_batch_returns_success_with_converted_text()
    {
        var command = CommandWith([ValidInstruction()]);

        var result = ConvertImtBatchHandler.Handle(command, Settings, TimeProvider.System);

        Assert.True(result.Success);
        Assert.False(string.IsNullOrEmpty(result.ConvertedText));
        Assert.Null(result.Errors);
    }

    [Fact]
    public void Invalid_batch_returns_failure_with_per_instruction_reasons()
    {
        var command = CommandWith([ValidInstruction() with { Notes = "" }]);

        var result = ConvertImtBatchHandler.Handle(command, Settings, TimeProvider.System);

        Assert.False(result.Success);
        Assert.Null(result.ConvertedText);
        Assert.NotNull(result.Errors);
        var error = Assert.Single(result.Errors);
        Assert.Equal(0, error.Index);
        Assert.Contains("Notes", error.Reason);
    }

    [Fact]
    public void A_spaced_account_number_and_overlong_bank_name_and_address_are_sanitized_not_rejected()
    {
        var command = CommandWith([ValidInstruction() with
        {
            DestinationBankAccountNo = "658 450191",
            IntermediaryBankName = "CHASE MANHATTAN BANK (J.P. MORGAN CHASE & CO)",
            BeneficiaryAddress = "9101 Alta Drive, Unit 15, Las Vegas, NV 89145",
        }]);

        var result = ConvertImtBatchHandler.Handle(command, Settings, TimeProvider.System);

        Assert.True(result.Success);
        var fields = result.ConvertedText!.Split(',');
        Assert.Equal("658450191", fields[17]); // field 18: Beneficiary - Account Number, space stripped
        Assert.Equal("CHASE MANHATTAN BANK J P MORGA", fields[10]); // field 11, sanitized then truncated to 30 chars
    }

    [Fact]
    public void Multi_instruction_batch_is_joined_with_CRLF_and_has_no_trailing_CRLF()
    {
        var first = ValidInstruction();
        var second = ValidInstruction() with { PaymentReference = "TT-000002" };
        var command = CommandWith([first, second]);

        var result = ConvertImtBatchHandler.Handle(command, Settings, TimeProvider.System);

        Assert.True(result.Success);
        var convertedText = result.ConvertedText!;

        Assert.False(convertedText.EndsWith("\r\n", StringComparison.Ordinal));
        var rows = convertedText.Split("\r\n");
        Assert.Equal(2, rows.Length); // exactly one row per instruction - no header/trailer, no blank trailing row
        Assert.Equal(ImtRecordMapper.Map(first, ImtRecordMapper.DeriveDebitAccountNumber(Settings)), rows[0]);
        Assert.Equal(ImtRecordMapper.Map(second, ImtRecordMapper.DeriveDebitAccountNumber(Settings)), rows[1]);
    }

    [Fact]
    public void Single_instruction_batch_has_no_CRLF_at_all()
    {
        var command = CommandWith([ValidInstruction()]);

        var result = ConvertImtBatchHandler.Handle(command, Settings, TimeProvider.System);

        Assert.DoesNotContain("\r\n", result.ConvertedText);
    }

    [Fact]
    public void Batch_exceeding_350_instructions_is_rejected_in_full()
    {
        var instructions = Enumerable.Repeat(ValidInstruction(), 351).ToArray();
        var command = CommandWith(instructions);

        var result = ConvertImtBatchHandler.Handle(command, Settings, TimeProvider.System);

        Assert.False(result.Success);
        Assert.Null(result.ConvertedText);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Index == -1 && e.Reason.Contains("at most 350"));
    }

    [Fact]
    public void Two_instruction_batch_mappings_has_one_row_per_instruction_in_order_no_header_or_trailer()
    {
        var first = ValidInstruction();
        var second = ValidInstruction() with { PaymentReference = "TT-000002" };
        var command = CommandWith([first, second]);

        var result = ConvertImtBatchHandler.Handle(command, Settings, TimeProvider.System);

        Assert.True(result.Success);
        Assert.Equal(["row1", "row2"], result.Mappings!.Select(line => line.Line));
    }

    [Fact]
    public void Row_mappings_fields_match_the_same_instruction_used_to_build_that_row_of_converted_text()
    {
        var first = ValidInstruction() with { PaymentReference = "TT-000001" };
        var second = ValidInstruction() with { PaymentReference = "TT-000002" };
        var command = CommandWith([first, second]);

        var result = ConvertImtBatchHandler.Handle(command, Settings, TimeProvider.System);

        var row1 = result.Mappings!.Single(line => line.Line == "row1");
        var row2 = result.Mappings!.Single(line => line.Line == "row2");
        Assert.Equal("TT-000001", row1.Fields.Single(f => f.CbaResponseField == "Beneficiary Payment Details").RequestValue);
        Assert.Equal("TT-000002", row2.Fields.Single(f => f.CbaResponseField == "Beneficiary Payment Details").RequestValue);
    }

    [Fact]
    public void Mappings_is_null_when_validation_fails()
    {
        var command = CommandWith([ValidInstruction() with { Notes = "" }]);

        var result = ConvertImtBatchHandler.Handle(command, Settings, TimeProvider.System);

        Assert.False(result.Success);
        Assert.Null(result.Mappings);
    }
}
