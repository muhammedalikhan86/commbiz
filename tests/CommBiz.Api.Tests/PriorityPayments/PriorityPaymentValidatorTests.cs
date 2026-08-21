using CommBiz.Api.Features.PriorityPayments;

namespace CommBiz.Api.Tests.PriorityPayments;

public class PriorityPaymentValidatorTests
{
    // Confirmed real sample payload from the Task Packet.
    private static PriorityPaymentInstructionRequest ValidInstruction() =>
        new(
            PaymentTypeCode: "RTGS",
            DestinationBankAccountName: "ORS APP GATB",
            DestinationBankAccountNo: "838629371",
            DestinationBankBsb: "012110",
            PaymentDate: DateTime.UtcNow.Date.AddDays(1),
            Amount: 10775.0m,
            Notes: "Accounts has been paid to before.",
            BeneficiaryAddress: null);

    private static IReadOnlyList<PriorityPaymentInstructionRequest> BatchWith(params PriorityPaymentInstructionRequest[] instructions) =>
        instructions;

    private static void AssertValid(IReadOnlyList<PriorityPaymentInstructionRequest> instructions) =>
        Assert.Null(PriorityPaymentValidator.Validate(instructions));

    private static void AssertSingleInvalidField(
        IReadOnlyList<PriorityPaymentInstructionRequest> instructions, int expectedIndex, string fieldNameSubstring)
    {
        var errors = PriorityPaymentValidator.Validate(instructions);
        Assert.NotNull(errors);
        var error = Assert.Single(errors);
        Assert.Equal(expectedIndex, error.Index);
        Assert.Contains(fieldNameSubstring, error.Reason);
    }

    [Fact]
    public void Fully_valid_instruction_passes() =>
        AssertValid(BatchWith(ValidInstruction()));

    [Fact]
    public void Null_BeneficiaryAddress_is_valid_since_the_field_is_optional() =>
        AssertValid(BatchWith(ValidInstruction() with { BeneficiaryAddress = null }));

    // --- Notes (Transaction Description) ---

    [Theory]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("Accounts has been paid to before.", true)]
    public void Notes_must_not_be_blank(string notes, bool expectedValid)
    {
        var batch = BatchWith(ValidInstruction() with { Notes = notes });

        if (expectedValid)
        {
            AssertValid(batch);
        }
        else
        {
            AssertSingleInvalidField(batch, 0, "Notes");
        }
    }

    // --- Process Date window (14 months ahead, not IMT's 7 days) ---

    [Fact]
    public void PaymentDate_in_the_past_is_rejected() =>
        AssertSingleInvalidField(
            BatchWith(ValidInstruction() with { PaymentDate = DateTime.UtcNow.Date.AddDays(-1) }), 0, "PaymentDate");

    [Fact]
    public void PaymentDate_today_is_valid() =>
        AssertValid(BatchWith(ValidInstruction() with { PaymentDate = DateTime.UtcNow.Date }));

    [Fact]
    public void PaymentDate_exactly_14_months_ahead_is_valid() =>
        AssertValid(BatchWith(ValidInstruction() with { PaymentDate = DateTime.UtcNow.Date.AddMonths(14) }));

    [Fact]
    public void PaymentDate_more_than_14_months_ahead_is_rejected() =>
        AssertSingleInvalidField(
            BatchWith(ValidInstruction() with { PaymentDate = DateTime.UtcNow.Date.AddMonths(14).AddDays(1) }), 0, "PaymentDate");

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

    // --- Beneficiary Bank BSB (field 14) ---

    [Theory]
    [InlineData("012110", true)]
    [InlineData("012-110", false)] // hyphen not allowed
    [InlineData("01211", false)] // too short
    [InlineData("0121100", false)] // too long
    [InlineData("", false)] // blank
    public void DestinationBankBsb_must_be_exactly_6_numeric_digits(string bsb, bool expectedValid)
    {
        var batch = BatchWith(ValidInstruction() with { DestinationBankBsb = bsb });

        if (expectedValid)
        {
            AssertValid(batch);
        }
        else
        {
            AssertSingleInvalidField(batch, 0, "DestinationBankBsb");
        }
    }

    // --- Beneficiary Account Number (field 18): 3AN to 9AN ---

    [Theory]
    [InlineData("838629371", true)] // 9 chars
    [InlineData("ABC", true)] // 3 chars, alphanumeric
    [InlineData("AB", false)] // too short
    [InlineData("1234567890", false)] // too long
    [InlineData("838-629371", false)] // hyphen not allowed
    public void DestinationBankAccountNo_must_be_3_to_9_alphanumeric_chars(string accountNo, bool expectedValid)
    {
        var batch = BatchWith(ValidInstruction() with { DestinationBankAccountNo = accountNo });

        if (expectedValid)
        {
            AssertValid(batch);
        }
        else
        {
            AssertSingleInvalidField(batch, 0, "DestinationBankAccountNo");
        }
    }

    // --- Beneficiary Account Name (field 19) - stricter than IMT: no hyphen/apostrophe ---

    [Theory]
    [InlineData("ORS APP GATB", true)]
    [InlineData("ORS APP GATB 2", true)]
    [InlineData("", false)] // blank
    [InlineData("O'Brien Smith", false)] // apostrophe not allowed
    [InlineData("Smith-Jones", false)] // hyphen not allowed
    public void DestinationBankAccountName_must_be_letters_numbers_spaces_only(string accountName, bool expectedValid)
    {
        var batch = BatchWith(ValidInstruction() with { DestinationBankAccountName = accountName });

        if (expectedValid)
        {
            AssertValid(batch);
        }
        else
        {
            AssertSingleInvalidField(batch, 0, "DestinationBankAccountName");
        }
    }

    [Fact]
    public void DestinationBankAccountName_over_32_chars_is_rejected() =>
        AssertSingleInvalidField(
            BatchWith(ValidInstruction() with { DestinationBankAccountName = new string('A', 33) }), 0, "DestinationBankAccountName");

    [Fact]
    public void DestinationBankAccountName_exactly_32_chars_is_valid() =>
        AssertValid(BatchWith(ValidInstruction() with { DestinationBankAccountName = new string('A', 32) }));

    // --- Beneficiary Address (field 20) - optional, but stricter than IMT's when present ---

    [Fact]
    public void BeneficiaryAddress_blank_string_is_valid_since_the_field_is_optional() =>
        AssertValid(BatchWith(ValidInstruction() with { BeneficiaryAddress = "" }));

    [Fact]
    public void BeneficiaryAddress_letters_numbers_spaces_is_valid() =>
        AssertValid(BatchWith(ValidInstruction() with { BeneficiaryAddress = "9101 Alta Drive U15" }));

    [Fact]
    public void BeneficiaryAddress_with_hyphen_is_rejected() =>
        AssertSingleInvalidField(
            BatchWith(ValidInstruction() with { BeneficiaryAddress = "9101 Alta-Drive" }), 0, "BeneficiaryAddress");

    [Fact]
    public void BeneficiaryAddress_over_40_chars_is_rejected() =>
        AssertSingleInvalidField(
            BatchWith(ValidInstruction() with { BeneficiaryAddress = new string('A', 41) }), 0, "BeneficiaryAddress");

    [Fact]
    public void BeneficiaryAddress_exactly_40_chars_is_valid() =>
        AssertValid(BatchWith(ValidInstruction() with { BeneficiaryAddress = new string('A', 40) }));

    // --- Batch-level instruction count ---

    private static IReadOnlyList<PriorityPaymentInstructionRequest> BatchWithCount(int count) =>
        Enumerable.Repeat(ValidInstruction(), count).ToArray();

    [Fact]
    public void Zero_instructions_is_rejected_with_minimum_count_reason()
    {
        var errors = PriorityPaymentValidator.Validate(BatchWithCount(0));

        Assert.NotNull(errors);
        Assert.Contains(errors, e => e.Index == -1 && e.Reason.Contains("at least 1"));
    }

    [Fact]
    public void Exactly_350_instructions_is_allowed() =>
        AssertValid(BatchWithCount(350));

    [Fact]
    public void More_than_350_instructions_is_rejected_with_max_count_reason()
    {
        var errors = PriorityPaymentValidator.Validate(BatchWithCount(351));

        Assert.NotNull(errors);
        Assert.Contains(errors, e => e.Index == -1 && e.Reason.Contains("at most 350"));
    }

    [Fact]
    public void Multiple_invalid_fields_across_multiple_instructions_are_all_reported()
    {
        var batch = BatchWith(
            ValidInstruction() with { Notes = "" },
            ValidInstruction() with { DestinationBankBsb = "" });

        var errors = PriorityPaymentValidator.Validate(batch);

        Assert.NotNull(errors);
        Assert.Equal(2, errors.Count);
        Assert.Equal(0, errors[0].Index);
        Assert.Equal(1, errors[1].Index);
    }
}
