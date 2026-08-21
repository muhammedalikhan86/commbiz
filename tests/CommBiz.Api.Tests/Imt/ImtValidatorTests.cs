using CommBiz.Api.Features.Imt;

namespace CommBiz.Api.Tests.Imt;

public class ImtValidatorTests
{
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

    private static IReadOnlyList<ImtPaymentInstructionRequest> BatchWith(params ImtPaymentInstructionRequest[] instructions) =>
        instructions;

    private static void AssertValid(IReadOnlyList<ImtPaymentInstructionRequest> instructions) =>
        Assert.Null(ImtValidator.Validate(instructions, TimeProvider.System));

    private static void AssertSingleInvalidField(
        IReadOnlyList<ImtPaymentInstructionRequest> instructions, int expectedIndex, string fieldNameSubstring)
    {
        var errors = ImtValidator.Validate(instructions, TimeProvider.System);
        Assert.NotNull(errors);
        var error = Assert.Single(errors);
        Assert.Equal(expectedIndex, error.Index);
        Assert.Contains(fieldNameSubstring, error.Reason);
    }

    [Fact]
    public void Fully_valid_instruction_passes() =>
        AssertValid(BatchWith(ValidInstruction()));

    // --- Notes (Transaction Description) ---

    [Theory]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("10 bps on fx", true)]
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

    // --- Process Date window ---

    [Fact]
    public void PaymentDate_in_the_past_is_rejected() =>
        AssertSingleInvalidField(
            BatchWith(ValidInstruction() with { PaymentDate = DateTime.UtcNow.Date.AddDays(-1) }), 0, "PaymentDate");

    [Fact]
    public void PaymentDate_today_is_valid() =>
        AssertValid(BatchWith(ValidInstruction() with { PaymentDate = DateTime.UtcNow.Date }));

    [Fact]
    public void PaymentDate_exactly_7_days_ahead_is_valid() =>
        AssertValid(BatchWith(ValidInstruction() with { PaymentDate = DateTime.UtcNow.Date.AddDays(7) }));

    [Fact]
    public void PaymentDate_more_than_7_days_ahead_is_rejected() =>
        AssertSingleInvalidField(
            BatchWith(ValidInstruction() with { PaymentDate = DateTime.UtcNow.Date.AddDays(8) }), 0, "PaymentDate");

    // --- Payment Currency ---

    [Theory]
    [InlineData("USD", true)]
    [InlineData("usd", false)] // must be upper case
    [InlineData("US", false)] // too short
    [InlineData("USDD", false)] // too long
    [InlineData("US1", false)] // non-letter
    public void SourceCurrency_must_be_exactly_3_upper_case_letters(string currency, bool expectedValid)
    {
        var batch = BatchWith(ValidInstruction() with { SourceCurrency = currency });

        if (expectedValid)
        {
            AssertValid(batch);
        }
        else
        {
            AssertSingleInvalidField(batch, 0, "SourceCurrency");
        }
    }

    // --- Payment Amount / Debit Amount mutual exclusivity ---

    [Fact]
    public void Both_amounts_zero_is_rejected()
    {
        var batch = BatchWith(ValidInstruction() with { SourceAmount = 0m, Amount = 0m });

        AssertSingleInvalidField(batch, 0, "SourceAmount");
    }

    [Fact]
    public void Both_amounts_populated_is_rejected()
    {
        var batch = BatchWith(ValidInstruction() with { SourceAmount = 100m, Amount = 200m });

        AssertSingleInvalidField(batch, 0, "SourceAmount");
    }

    [Fact]
    public void Only_SourceAmount_populated_passes() =>
        AssertValid(BatchWith(ValidInstruction() with { SourceAmount = 588517.58m, Amount = 0m }));

    [Fact]
    public void Only_Amount_populated_passes() =>
        AssertValid(BatchWith(ValidInstruction() with { SourceAmount = 0m, Amount = 588517.58m }));

    [Fact]
    public void Populated_amount_with_more_than_2_decimal_digits_is_rejected() =>
        AssertSingleInvalidField(BatchWith(ValidInstruction() with { SourceAmount = 100.123m, Amount = 0m }), 0, "amount");

    [Fact]
    public void Populated_amount_with_more_than_11_integer_digits_is_rejected() =>
        AssertSingleInvalidField(
            BatchWith(ValidInstruction() with { SourceAmount = 100_000_000_000.00m, Amount = 0m }), 0, "amount");

    // --- SWIFT codes (fields 10/14) ---

    [Theory]
    [InlineData("CHASUS33", true)] // 8 chars
    [InlineData("CHASUS33XXX", true)] // 11 chars
    [InlineData("CHASUS3", false)] // 7 chars
    [InlineData("CHASUS33X", false)] // 9 chars
    [InlineData("CHAS-US3", false)] // non-alphanumeric
    public void DestinationBankSwiftCode_must_be_8_or_11_alphanumeric_chars(string swiftCode, bool expectedValid)
    {
        var batch = BatchWith(ValidInstruction() with { DestinationBankSwiftCode = swiftCode });

        if (expectedValid)
        {
            AssertValid(batch);
        }
        else
        {
            AssertSingleInvalidField(batch, 0, "DestinationBankSwiftCode");
        }
    }

    [Fact]
    public void IntermediaryBankSwiftCode_null_is_valid_since_field_10_is_optional() =>
        AssertValid(BatchWith(ValidInstruction() with { IntermediaryBankSwiftCode = null }));

    [Fact]
    public void IntermediaryBankSwiftCode_present_but_invalid_length_is_rejected() =>
        AssertSingleInvalidField(
            BatchWith(ValidInstruction() with { IntermediaryBankSwiftCode = "BAD" }), 0, "IntermediaryBankSwiftCode");

    // --- Bank name length (fields 11/15) ---

    [Fact]
    public void DestinationBankName_over_30_chars_is_rejected() =>
        AssertSingleInvalidField(
            BatchWith(ValidInstruction() with { DestinationBankName = new string('A', 31) }), 0, "DestinationBankName");

    [Fact]
    public void DestinationBankName_exactly_30_chars_is_valid() =>
        AssertValid(BatchWith(ValidInstruction() with { DestinationBankName = new string('A', 30) }));

    [Fact]
    public void IntermediaryBankName_over_30_chars_is_no_longer_rejected_by_Validate_itself() =>
        AssertValid(BatchWith(ValidInstruction() with { IntermediaryBankName = new string('A', 31) }));

    [Fact]
    public void IntermediaryBankName_null_is_valid_since_field_11_is_optional() =>
        AssertValid(BatchWith(ValidInstruction() with { IntermediaryBankName = null }));

    // --- Beneficiary Account Number (field 18) ---

    [Theory]
    [InlineData("658450191", true)]
    [InlineData("6584 50191", true)] // no longer rejected by Validate - Sanitize strips the space first
    [InlineData("6584-50191", true)] // no longer rejected by Validate - Sanitize strips the hyphen first
    [InlineData("6584,50191", true)] // no longer rejected by Validate - Sanitize strips the comma first
    [InlineData("", false)] // blank
    public void DestinationBankAccountNo_separators_are_no_longer_rejected_by_Validate_itself(string accountNo, bool expectedValid)
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

    [Fact]
    public void DestinationBankAccountNo_over_34_chars_is_rejected() =>
        AssertSingleInvalidField(
            BatchWith(ValidInstruction() with { DestinationBankAccountNo = new string('1', 35) }), 0, "DestinationBankAccountNo");

    // --- Beneficiary Account Name (field 19) ---

    [Theory]
    [InlineData("SAMER MOHAMMED KIKI", true)]
    [InlineData("O'Brien-Smith", true)]
    [InlineData("12345", false)] // no letters
    [InlineData("SAMER, KIKI", false)] // disallowed comma
    [InlineData("SAMER & KIKI", false)] // disallowed ampersand
    public void DestinationBankAccountName_must_contain_a_letter_and_only_allowed_chars(string accountName, bool expectedValid)
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
    public void DestinationBankAccountName_over_62_chars_is_rejected() =>
        AssertSingleInvalidField(
            BatchWith(ValidInstruction() with { DestinationBankAccountName = new string('A', 63) }), 0, "DestinationBankAccountName");

    // --- Beneficiary Address (field 20) - sanitize, then validate length ---

    [Fact]
    public void BeneficiaryAddress_with_commas_is_sanitized_not_rejected() =>
        AssertValid(BatchWith(ValidInstruction() with { BeneficiaryAddress = "9101 Alta Drive, U15, Las Vegas, NV 89145" }));

    [Fact]
    public void BeneficiaryAddress_blank_is_rejected() =>
        AssertSingleInvalidField(BatchWith(ValidInstruction() with { BeneficiaryAddress = "" }), 0, "BeneficiaryAddress");

    [Fact]
    public void BeneficiaryAddress_length_is_no_longer_rejected_by_Validate_itself() =>
        AssertValid(BatchWith(ValidInstruction() with { BeneficiaryAddress = new string('A', 41) }));

    // --- Payment Details (field 27) - sanitize, then validate length ---

    [Fact]
    public void PaymentReference_with_ampersand_is_sanitized_not_rejected() =>
        AssertValid(BatchWith(ValidInstruction() with { PaymentReference = "Invoice & Ref 123" }));

    [Fact]
    public void PaymentReference_blank_is_rejected() =>
        AssertSingleInvalidField(BatchWith(ValidInstruction() with { PaymentReference = "" }), 0, "PaymentReference");

    [Fact]
    public void PaymentReference_still_too_long_after_sanitization_is_rejected() =>
        AssertSingleInvalidField(
            BatchWith(ValidInstruction() with { PaymentReference = new string('A', 106) }), 0, "PaymentReference");

    // --- Batch-level instruction count ---

    private static IReadOnlyList<ImtPaymentInstructionRequest> BatchWithCount(int count) =>
        Enumerable.Repeat(ValidInstruction(), count).ToArray();

    [Fact]
    public void Zero_instructions_is_rejected_with_minimum_count_reason()
    {
        var errors = ImtValidator.Validate(BatchWithCount(0), TimeProvider.System);

        Assert.NotNull(errors);
        Assert.Contains(errors, e => e.Index == -1 && e.Reason.Contains("at least 1"));
    }

    [Fact]
    public void Exactly_350_instructions_is_allowed() =>
        AssertValid(BatchWithCount(350));

    [Fact]
    public void More_than_350_instructions_is_rejected_with_max_count_reason()
    {
        var errors = ImtValidator.Validate(BatchWithCount(351), TimeProvider.System);

        Assert.NotNull(errors);
        Assert.Contains(errors, e => e.Index == -1 && e.Reason.Contains("at most 350"));
    }

    [Fact]
    public void Multiple_invalid_fields_across_multiple_instructions_are_all_reported()
    {
        var batch = BatchWith(
            ValidInstruction() with { Notes = "" },
            ValidInstruction() with { PaymentReference = "" });

        var errors = ImtValidator.Validate(batch, TimeProvider.System);

        Assert.NotNull(errors);
        Assert.Equal(2, errors.Count);
        Assert.Equal(0, errors[0].Index);
        Assert.Equal(1, errors[1].Index);
    }

    // --- Sanitize (runs before Validate - replaces what used to be reject-on-invalid rules) ---

    [Theory]
    [InlineData("6584 50191", "658450191")]
    [InlineData("6584-50191", "658450191")]
    [InlineData("6584,50191", "658450191")]
    public void Sanitize_strips_spaces_hyphens_and_commas_from_DestinationBankAccountNo(string accountNo, string expected)
    {
        var sanitized = ImtValidator.Sanitize(ValidInstruction() with { DestinationBankAccountNo = accountNo });

        Assert.Equal(expected, sanitized.DestinationBankAccountNo);
    }

    [Fact]
    public void Sanitize_truncates_IntermediaryBankName_to_30_characters()
    {
        var sanitized = ImtValidator.Sanitize(ValidInstruction() with { IntermediaryBankName = new string('A', 31) });

        Assert.Equal(new string('A', 30), sanitized.IntermediaryBankName);
    }

    [Fact]
    public void Sanitize_removes_disallowed_characters_from_IntermediaryBankName()
    {
        var sanitized = ImtValidator.Sanitize(ValidInstruction() with { IntermediaryBankName = "J.P. Morgan & Co, Ltd." });

        Assert.Equal("J P Morgan Co Ltd", sanitized.IntermediaryBankName);
    }

    [Fact]
    public void Sanitize_leaves_null_IntermediaryBankName_as_null() =>
        Assert.Null(ImtValidator.Sanitize(ValidInstruction() with { IntermediaryBankName = null }).IntermediaryBankName);

    [Fact]
    public void Sanitize_removes_disallowed_characters_and_truncates_BeneficiaryAddress_to_40_characters()
    {
        var sanitized = ImtValidator.Sanitize(
            ValidInstruction() with { BeneficiaryAddress = "9101 Alta Drive, Unit 15, Las Vegas, NV 89145" });

        Assert.Equal(40, sanitized.BeneficiaryAddress.Length);
        Assert.Equal("9101 Alta Drive Unit 15 Las Vegas NV 891", sanitized.BeneficiaryAddress);
    }

    [Fact]
    public void Sanitized_over_length_fields_pass_Validate_afterwards()
    {
        var sanitized = ImtValidator.Sanitize(ValidInstruction() with
        {
            DestinationBankAccountNo = "6584 50191",
            IntermediaryBankName = new string('A', 31),
            BeneficiaryAddress = new string('A', 41),
        });

        AssertValid(BatchWith(sanitized));
    }
}
