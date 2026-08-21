using CommBiz.Api.Features.Fx;

namespace CommBiz.Api.Tests.Fx;

public class FxValidatorTests
{
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

    private static IReadOnlyList<FxPaymentInstructionRequest> BatchWith(params FxPaymentInstructionRequest[] instructions) =>
        instructions;

    private static void AssertValid(IReadOnlyList<FxPaymentInstructionRequest> instructions) =>
        Assert.Null(FxValidator.Validate(instructions));

    private static void AssertSingleInvalidField(
        IReadOnlyList<FxPaymentInstructionRequest> instructions, int expectedIndex, string fieldNameSubstring)
    {
        var errors = FxValidator.Validate(instructions);
        Assert.NotNull(errors);
        var error = Assert.Single(errors);
        Assert.Equal(expectedIndex, error.Index);
        Assert.Contains(fieldNameSubstring, error.Reason);
    }

    [Fact]
    public void Fully_valid_instruction_passes() =>
        AssertValid(BatchWith(ValidInstruction()));

    // --- BuyCurrency / SellCurrency ---

    [Theory]
    [InlineData("USD", true)]
    [InlineData("usd", false)] // lowercase not allowed
    [InlineData("US", false)] // too short
    [InlineData("USDD", false)] // too long
    [InlineData("", false)] // blank
    public void BuyCurrency_must_be_exactly_3_uppercase_letters(string buyCurrency, bool expectedValid)
    {
        var batch = BatchWith(ValidInstruction() with { BuyCurrency = buyCurrency });

        if (expectedValid)
        {
            AssertValid(batch);
        }
        else
        {
            AssertSingleInvalidField(batch, 0, "BuyCurrency");
        }
    }

    [Theory]
    [InlineData("AUD", true)]
    [InlineData("aud", false)] // lowercase not allowed
    [InlineData("AU", false)] // too short
    [InlineData("AUDD", false)] // too long
    [InlineData("", false)] // blank
    public void SellCurrency_must_be_exactly_3_uppercase_letters(string sellCurrency, bool expectedValid)
    {
        var batch = BatchWith(ValidInstruction() with { SellCurrency = sellCurrency });

        if (expectedValid)
        {
            AssertValid(batch);
        }
        else
        {
            AssertSingleInvalidField(batch, 0, "SellCurrency");
        }
    }

    // --- Amount ---

    [Fact]
    public void Amount_of_zero_is_rejected() =>
        AssertSingleInvalidField(BatchWith(ValidInstruction() with { Amount = 0m }), 0, "Amount");

    [Fact]
    public void Negative_amount_is_rejected() =>
        AssertSingleInvalidField(BatchWith(ValidInstruction() with { Amount = -1m }), 0, "Amount");

    [Fact]
    public void Amount_with_more_than_2_decimal_digits_is_rejected() =>
        AssertSingleInvalidField(BatchWith(ValidInstruction() with { Amount = 100.123m }), 0, "Amount");

    [Fact]
    public void Amount_with_more_than_11_integer_digits_is_rejected() =>
        AssertSingleInvalidField(BatchWith(ValidInstruction() with { Amount = 100_000_000_000.00m }), 0, "Amount");

    // --- AccountNo (Transaction Description, field 2) ---

    [Theory]
    [InlineData("Payment2", true)]
    [InlineData("A", true)] // 1 char - minimum
    [InlineData("123456789012", true)] // 12 chars - maximum
    [InlineData("", false)] // blank
    [InlineData("1234567890123", false)] // 13 chars - too long
    [InlineData("Payment 2", false)] // space not allowed
    public void AccountNo_must_be_1_to_12_alphanumeric_chars(string accountNo, bool expectedValid)
    {
        var batch = BatchWith(ValidInstruction() with { AccountNo = accountNo });

        if (expectedValid)
        {
            AssertValid(batch);
        }
        else
        {
            AssertSingleInvalidField(batch, 0, "AccountNo");
        }
    }

    // --- Batch-level instruction count ---

    private static IReadOnlyList<FxPaymentInstructionRequest> BatchWithCount(int count) =>
        Enumerable.Repeat(ValidInstruction(), count).ToArray();

    [Fact]
    public void Zero_instructions_is_rejected_with_minimum_count_reason()
    {
        var errors = FxValidator.Validate(BatchWithCount(0));

        Assert.NotNull(errors);
        Assert.Contains(errors, e => e.Index == -1 && e.Reason.Contains("at least 1"));
    }

    [Fact]
    public void Exactly_1_instruction_is_allowed() =>
        AssertValid(BatchWithCount(1));

    [Fact]
    public void Exactly_200_instructions_is_allowed() =>
        AssertValid(BatchWithCount(200));

    [Fact]
    public void More_than_200_instructions_is_rejected_with_max_count_reason()
    {
        var errors = FxValidator.Validate(BatchWithCount(201));

        Assert.NotNull(errors);
        Assert.Contains(errors, e => e.Index == -1 && e.Reason.Contains("at most 200"));
    }

    // --- Batch-level distinct currency pair count ---

    // Buy currency varies per pair (still 3 uppercase letters, per the currency code format rule)
    // while Sell currency stays fixed - each pair is distinct, and every code stays format-valid.
    private static FxPaymentInstructionRequest[] BatchWithDistinctCurrencyPairs(int pairCount) =>
        Enumerable.Range(0, pairCount)
            .Select(i => ValidInstruction() with { BuyCurrency = $"A{(char)('A' + i)}A", SellCurrency = "AUD" })
            .ToArray();

    [Fact]
    public void Exactly_15_distinct_currency_pairs_is_allowed() =>
        AssertValid(BatchWithDistinctCurrencyPairs(15));

    [Fact]
    public void More_than_15_distinct_currency_pairs_is_rejected_with_pair_count_reason()
    {
        var errors = FxValidator.Validate(BatchWithDistinctCurrencyPairs(16));

        Assert.NotNull(errors);
        Assert.Contains(errors, e => e.Index == -1 && e.Reason.Contains("at most 15 distinct currency pairs"));
    }

    [Fact]
    public void Repeated_currency_pairs_across_instructions_count_once()
    {
        var batch = BatchWith(ValidInstruction(), ValidInstruction());

        AssertValid(batch);
    }

    [Fact]
    public void Multiple_invalid_fields_across_multiple_instructions_are_all_reported()
    {
        var batch = BatchWith(
            ValidInstruction() with { AccountNo = "" },
            ValidInstruction() with { BuyCurrency = "usd" });

        var errors = FxValidator.Validate(batch);

        Assert.NotNull(errors);
        Assert.Equal(2, errors.Count);
        Assert.Equal(0, errors[0].Index);
        Assert.Equal(1, errors[1].Index);
    }
}
