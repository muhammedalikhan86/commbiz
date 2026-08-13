using CommBiz.Api.Features.DirectEntry;

namespace CommBiz.Api.Tests;

public class DirectEntryValidatorTests
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

    private static PaymentInstructionRequest ValidInstruction() =>
        new(
            PaymentTypeCode: "DE",
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

    private static IReadOnlyList<PaymentInstructionRequest> BatchWith(params PaymentInstructionRequest[] instructions)
    {
        // Field-focused tests below only care about a single instruction's data; pad up to F-008's
        // minimum-2 rule with extra valid instructions so those tests aren't tripped by that rule.
        return instructions.Length >= 2
            ? instructions
            : [.. instructions, .. Enumerable.Repeat(ValidInstruction(), 2 - instructions.Length)];
    }

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

    // --- SourceBankBsb ---

    [Theory]
    [InlineData("015141", true)]
    [InlineData("015-141", false)] // hyphen not allowed on the raw field
    [InlineData("01514", false)] // 5 digits, too short
    [InlineData("0151411", false)] // 7 digits, too long
    [InlineData("01514A", false)] // non-numeric
    [InlineData("", false)]
    public void SourceBankBsb_must_be_exactly_6_numeric_digits(string sourceBankBsb, bool expectedValid)
    {
        var batch = BatchWith(ValidInstruction() with { SourceBankBsb = sourceBankBsb });

        if (expectedValid)
        {
            AssertValid(batch);
        }
        else
        {
            AssertSingleInvalidField(batch, 0, "SourceBankBsb");
        }
    }

    // --- SourceBankAccountNo ---

    [Theory]
    [InlineData("111375004", true)]
    [InlineData("123456789", true)] // exactly 9 chars, boundary
    [InlineData("1234567890", false)] // 10 chars, over max
    [InlineData("12345$678", false)] // disallowed character
    [InlineData("000000000", false)] // all zeros
    [InlineData("", false)] // all blank
    [InlineData("   ", false)] // all blank (spaces)
    public void SourceBankAccountNo_rules_are_enforced(string sourceBankAccountNo, bool expectedValid)
    {
        var batch = BatchWith(ValidInstruction() with { SourceBankAccountNo = sourceBankAccountNo });

        if (expectedValid)
        {
            AssertValid(batch);
        }
        else
        {
            AssertSingleInvalidField(batch, 0, "SourceBankAccountNo");
        }
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

    // --- AccountNo ---

    [Fact]
    public void AccountNo_blank_is_invalid() =>
        AssertSingleInvalidField(BatchWith(ValidInstruction() with { AccountNo = "" }), 0, "AccountNo");

    [Fact]
    public void AccountNo_populated_is_valid() =>
        AssertValid(BatchWith(ValidInstruction() with { AccountNo = "S1605677" }));

    // --- Minimum instruction count (F-008): spec requires at least 2 detail records ---

    private static IReadOnlyList<PaymentInstructionRequest> BatchWithCount(int count) =>
        Enumerable.Repeat(ValidInstruction(), count).ToArray();

    [Fact]
    public void Zero_instructions_is_rejected_with_minimum_count_reason()
    {
        var errors = DirectEntryValidator.Validate(BatchWithCount(0));

        Assert.NotNull(errors);
        Assert.Contains(errors, e => e.Index == -1 && e.Reason.Contains("at least 2"));
    }

    [Fact]
    public void One_instruction_is_rejected_with_minimum_count_reason()
    {
        var errors = DirectEntryValidator.Validate(BatchWithCount(1));

        Assert.NotNull(errors);
        Assert.Contains(errors, e => e.Index == -1 && e.Reason.Contains("at least 2"));
    }

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
        var batch = BatchWith(ValidInstruction() with { SourceBankBsb = "bad", AccountNo = "" });

        var errors = DirectEntryValidator.Validate(batch);

        Assert.NotNull(errors);
        Assert.Equal(2, errors.Count);
        Assert.All(errors, e => Assert.Equal(0, e.Index));
        Assert.Contains(errors, e => e.Reason.Contains("SourceBankBsb"));
        Assert.Contains(errors, e => e.Reason.Contains("AccountNo"));
    }

    [Fact]
    public void F004_router_rejection_short_circuits_F005_validation()
    {
        // Invalid payment type AND an invalid field on the same instruction: only the router's
        // error should surface — field validation must not run once routing has already rejected.
        var command = new ConvertDirectEntryBatchCommand(
            BatchWith(ValidInstruction() with { PaymentTypeCode = "BPAY", SourceBankBsb = "bad" }));

        var result = ConvertDirectEntryBatchHandler.Handle(command, Settings);

        Assert.False(result.Success);
        Assert.Null(result.ConvertedText);
        Assert.NotNull(result.Errors);
        var error = Assert.Single(result.Errors);
        Assert.Equal(0, error.Index);
        Assert.Contains("Unsupported payment type", error.Reason);
    }
}

