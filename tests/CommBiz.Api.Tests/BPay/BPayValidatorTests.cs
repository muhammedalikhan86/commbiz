using CommBiz.Api.Features.BPay;

namespace CommBiz.Api.Tests.BPay;

public class BPayValidatorTests
{
    private static BPayPaymentInstructionRequest ValidInstruction() =>
        new(
            PaymentTypeCode: "BPAY",
            AccountNo: "S1218937",
            PaymentDate: DateTime.UtcNow.Date.AddDays(1),
            Amount: 10000.00m,
            BPayBillerCode: "488577",
            BPayReference: "1202194308172118");

    private static IReadOnlyList<BPayPaymentInstructionRequest> BatchWith(params BPayPaymentInstructionRequest[] instructions) =>
        instructions;

    private static void AssertValid(IReadOnlyList<BPayPaymentInstructionRequest> instructions) =>
        Assert.Null(BPayValidator.Validate(instructions));

    private static void AssertSingleInvalidField(
        IReadOnlyList<BPayPaymentInstructionRequest> instructions, int expectedIndex, string fieldNameSubstring)
    {
        var errors = BPayValidator.Validate(instructions);
        Assert.NotNull(errors);
        var error = Assert.Single(errors);
        Assert.Equal(expectedIndex, error.Index);
        Assert.Contains(fieldNameSubstring, error.Reason);
    }

    // --- BPayBillerCode ---

    [Theory]
    [InlineData("488577", true)]
    [InlineData("1234567890", true)] // exactly 10 digits, boundary
    [InlineData("12345678901", false)] // 11 digits, over max
    [InlineData("48857A", false)] // non-numeric
    [InlineData("", false)] // missing
    public void BPayBillerCode_must_be_numeric_1_to_10_digits(string billerCode, bool expectedValid)
    {
        var batch = BatchWith(ValidInstruction() with { BPayBillerCode = billerCode });

        if (expectedValid)
        {
            AssertValid(batch);
        }
        else
        {
            AssertSingleInvalidField(batch, 0, "BPayBillerCode");
        }
    }

    // --- BPayReference ---

    [Theory]
    [InlineData("1202194308172118", true)]
    [InlineData("12345678901234567890", true)] // exactly 20 digits, boundary
    [InlineData("123456789012345678901", false)] // 21 digits, over max
    [InlineData("1202194A", false)] // non-numeric
    [InlineData("", false)] // missing
    public void BPayReference_must_be_numeric_1_to_20_digits(string reference, bool expectedValid)
    {
        var batch = BatchWith(ValidInstruction() with { BPayReference = reference });

        if (expectedValid)
        {
            AssertValid(batch);
        }
        else
        {
            AssertSingleInvalidField(batch, 0, "BPayReference");
        }
    }

    // --- Amount ---

    [Theory]
    [InlineData(10000.00, true)]
    [InlineData(9_999_999_999.99, true)] // converts to exactly 12 digits of cents, boundary
    [InlineData(0, false)] // not positive
    [InlineData(-1, false)] // not positive
    [InlineData(10_000_000_000.00, false)] // converts to 13 digits of cents, over max
    public void Amount_must_be_positive_and_within_12_digits_of_cents(decimal amount, bool expectedValid)
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
        AssertValid(BatchWith(ValidInstruction() with { AccountNo = "S1218937" }));

    // --- PaymentDate ---

    [Fact]
    public void PaymentDate_today_is_valid() =>
        AssertValid(BatchWith(ValidInstruction() with { PaymentDate = DateTime.UtcNow.Date }));

    [Fact]
    public void PaymentDate_15_months_ahead_is_valid() =>
        AssertValid(BatchWith(ValidInstruction() with { PaymentDate = DateTime.UtcNow.Date.AddMonths(15) }));

    [Fact]
    public void PaymentDate_beyond_15_months_ahead_is_invalid() =>
        AssertSingleInvalidField(
            BatchWith(ValidInstruction() with { PaymentDate = DateTime.UtcNow.Date.AddMonths(15).AddDays(1) }),
            0,
            "PaymentDate");

    [Fact]
    public void PaymentDate_in_the_past_is_invalid() =>
        AssertSingleInvalidField(
            BatchWith(ValidInstruction() with { PaymentDate = DateTime.UtcNow.Date.AddDays(-1) }),
            0,
            "PaymentDate");

    // --- Batch-level instruction count ---

    private static IReadOnlyList<BPayPaymentInstructionRequest> BatchWithCount(int count) =>
        Enumerable.Repeat(ValidInstruction(), count).ToArray();

    [Fact]
    public void Zero_instructions_is_rejected_with_minimum_count_reason()
    {
        var errors = BPayValidator.Validate(BatchWithCount(0));

        Assert.NotNull(errors);
        Assert.Contains(errors, e => e.Index == -1 && e.Reason.Contains("at least 1"));
    }

    [Fact]
    public void One_instruction_is_not_rejected_on_minimum_count_ground() =>
        AssertValid(BatchWithCount(1));

    [Fact]
    public void Exactly_200_instructions_is_allowed() =>
        AssertValid(BatchWithCount(200));

    [Fact]
    public void More_than_200_instructions_is_rejected_with_max_count_reason()
    {
        var errors = BPayValidator.Validate(BatchWithCount(201));

        Assert.NotNull(errors);
        Assert.Contains(errors, e => e.Index == -1 && e.Reason.Contains("at most 200"));
    }

    // --- Cross-cutting behaviour ---

    [Fact]
    public void Fully_valid_batch_proceeds_to_conversion() =>
        AssertValid(BatchWith(ValidInstruction(), ValidInstruction()));

    [Fact]
    public void Multiple_invalid_fields_across_multiple_instructions_are_all_reported()
    {
        var batch = BatchWith(
            ValidInstruction() with { BPayBillerCode = "" },
            ValidInstruction() with { BPayReference = "" });

        var errors = BPayValidator.Validate(batch);

        Assert.NotNull(errors);
        Assert.Equal(2, errors.Count);
        Assert.Equal(0, errors[0].Index);
        Assert.Equal(1, errors[1].Index);
    }
}
