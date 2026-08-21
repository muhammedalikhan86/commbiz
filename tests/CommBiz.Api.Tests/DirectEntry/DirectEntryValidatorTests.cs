using CommBiz.Api.Features.DirectEntry;

namespace CommBiz.Api.Tests.DirectEntry;

public class DirectEntryValidatorTests
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

    private static PaymentInstructionRequest ValidInstruction() =>
        new(
            PaymentTypeCode: "DE",
            DestinationBankBsb: "484799",
            DestinationBankAccountNo: "300500",
            DestinationBankAccountName: "JOHN CITIZEN",
            PaymentDate: new DateTime(2026, 8, 20, 10, 0, 0),
            Amount: 7500.0m);

    // F-014 dropped the request-level minimum to 1, so field-focused tests below can use a single
    // instruction directly without needing padding to satisfy a higher minimum-count rule.
    private static IReadOnlyList<PaymentInstructionRequest> BatchWith(params PaymentInstructionRequest[] instructions) =>
        instructions;

    private static void AssertValid(IReadOnlyList<PaymentInstructionRequest> instructions) =>
        Assert.Null(DirectEntryValidator.Validate(instructions));

    private static void AssertSingleInvalidField(
        IReadOnlyList<PaymentInstructionRequest> instructions, int expectedIndex, string fieldNameSubstring)
    {
        var errors = DirectEntryValidator.Validate(instructions);
        Assert.NotNull(errors);
        var error = Assert.Single(errors);
        Assert.Equal(expectedIndex, error.Index);
        Assert.Contains(fieldNameSubstring, error.Reason);
    }

    // --- Amount ---

    [Theory]
    [InlineData(7500.0, true)]
    [InlineData(99_999_999.99, true)] // converts to exactly 10 digits of cents, boundary
    [InlineData(0, false)] // not positive
    [InlineData(-1, false)] // not positive
    [InlineData(100_000_000.00, false)] // converts to 11 digits of cents, over max
    public void Amount_must_be_positive_and_within_10_digits_of_cents(decimal amount, bool expectedValid)
    {
        var batch = BatchWith(ValidInstruction() with { Amount = amount });

        if (expectedValid)
        {
            AssertValid(batch);
        }
        else
        {
            AssertSingleInvalidField(batch, 0, "Amount");
        }
    }

    // --- DestinationBankBsb ---

    [Theory]
    [InlineData("484799", true)]
    [InlineData("484-799", false)] // hyphen not allowed on the raw field
    [InlineData("48479", false)] // 5 digits, too short
    [InlineData("4847991", false)] // 7 digits, too long
    [InlineData("48479A", false)] // non-numeric
    [InlineData("", false)]
    public void DestinationBankBsb_must_be_exactly_6_numeric_digits(string destinationBankBsb, bool expectedValid)
    {
        var batch = BatchWith(ValidInstruction() with { DestinationBankBsb = destinationBankBsb });

        if (expectedValid)
        {
            AssertValid(batch);
        }
        else
        {
            AssertSingleInvalidField(batch, 0, "DestinationBankBsb");
        }
    }

    // --- DestinationBankAccountNo ---

    [Theory]
    [InlineData("300500", true)]
    [InlineData("123456789", true)] // exactly 9 chars, boundary
    [InlineData("1234567890", false)] // 10 chars, over max
    [InlineData("12345$678", false)] // disallowed character
    [InlineData("000000000", false)] // all zeros
    [InlineData("", false)] // all blank
    [InlineData("   ", false)] // all blank (spaces)
    public void DestinationBankAccountNo_rules_are_enforced(string destinationBankAccountNo, bool expectedValid)
    {
        var batch = BatchWith(ValidInstruction() with { DestinationBankAccountNo = destinationBankAccountNo });

        if (expectedValid)
        {
            AssertValid(batch);
        }
        else
        {
            AssertSingleInvalidField(batch, 0, "DestinationBankAccountNo");
        }
    }

    // --- DestinationBankAccountName ---

    [Fact]
    public void DestinationBankAccountName_blank_is_invalid() =>
        AssertSingleInvalidField(
            BatchWith(ValidInstruction() with { DestinationBankAccountName = "" }), 0, "DestinationBankAccountName");

    [Fact]
    public void DestinationBankAccountName_populated_is_valid() =>
        AssertValid(BatchWith(ValidInstruction() with { DestinationBankAccountName = "JOHN CITIZEN" }));

    // --- Minimum instruction count (F-014): self-balancing record guarantees the output's own
    // >=2 detail record structural rule, so the request-level minimum is now 1 payment instruction ---

    private static IReadOnlyList<PaymentInstructionRequest> BatchWithCount(int count) =>
        Enumerable.Repeat(ValidInstruction(), count).ToArray();

    [Fact]
    public void Zero_instructions_is_rejected_with_minimum_count_reason()
    {
        var errors = DirectEntryValidator.Validate(BatchWithCount(0));

        Assert.NotNull(errors);
        Assert.Contains(errors, e => e.Index == -1 && e.Reason.Contains("at least 1"));
    }

    [Fact]
    public void One_instruction_is_not_rejected_on_minimum_count_ground() =>
        AssertValid(BatchWithCount(1)); // fully valid data otherwise -> no errors at all

    [Fact]
    public void Two_instructions_are_not_rejected_on_minimum_count_ground() =>
        AssertValid(BatchWithCount(2)); // fully valid data otherwise -> no errors at all

    // --- Cross-cutting behaviour ---

    [Fact]
    public void Fully_valid_batch_proceeds_to_conversion()
    {
        var command = new ConvertDirectEntryBatchCommand(BatchWith(ValidInstruction()));

        var result = ConvertDirectEntryBatchHandler.Handle(command, Settings);

        Assert.True(result.Success);
        Assert.False(string.IsNullOrEmpty(result.ConvertedText));
        Assert.Null(result.Errors);
    }

    [Fact]
    public void Multiple_simultaneous_field_failures_on_one_instruction_are_all_reported()
    {
        var batch = BatchWith(ValidInstruction() with { DestinationBankBsb = "bad", DestinationBankAccountName = "" });

        var errors = DirectEntryValidator.Validate(batch);

        Assert.NotNull(errors);
        Assert.Equal(2, errors.Count);
        Assert.All(errors, e => Assert.Equal(0, e.Index));
        Assert.Contains(errors, e => e.Reason.Contains("DestinationBankBsb"));
        Assert.Contains(errors, e => e.Reason.Contains("DestinationBankAccountName"));
    }

}

