using CommBiz.Api.Features.DirectEntry;

namespace CommBiz.Api.Tests;

public class DirectEntryValidatorTests
{
    private static PaymentInstructionRequest ValidInstruction() =>
        new(
            PaymentType: "DirectEntry",
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

    private static ConvertDirectEntryBatchRequest ValidRequest(params PaymentInstructionRequest[] instructions)
    {
        // Field-focused tests below only care about a single instruction's data; pad up to F-008's
        // minimum-2 rule with extra valid instructions so those tests aren't tripped by that rule.
        PaymentInstructionRequest[] padded = instructions.Length >= 2
            ? instructions
            : [.. instructions, .. Enumerable.Repeat(ValidInstruction(), 2 - instructions.Length)];

        return new(
            FileName: "COMPANY ABCD PTY LTD",
            UserIdentificationNumber: "301500",
            DescriptionOfEntries: "EFT-PAYMENT",
            DateToBeProcessed: new DateOnly(2026, 12, 5),
            Instructions: padded);
    }

    private static void AssertValid(ConvertDirectEntryBatchRequest request) =>
        Assert.Null(DirectEntryValidator.Validate(request));

    private static void AssertSingleInvalidField(
        ConvertDirectEntryBatchRequest request, int expectedIndex, string fieldNameSubstring)
    {
        var errors = DirectEntryValidator.Validate(request);
        Assert.NotNull(errors);
        var error = Assert.Single(errors);
        Assert.Equal(expectedIndex, error.Index);
        Assert.Contains(fieldNameSubstring, error.Reason);
    }

    // --- Bsb / TraceBsb ---

    [Theory]
    [InlineData("062-000", true)]
    [InlineData("062000", false)]
    [InlineData("62-000", false)]
    [InlineData("062-00A", false)]
    [InlineData("", false)]
    public void Bsb_must_match_nnn_nnn_format(string bsb, bool expectedValid)
    {
        var request = ValidRequest(ValidInstruction() with { Bsb = bsb });

        if (expectedValid)
        {
            AssertValid(request);
        }
        else
        {
            AssertSingleInvalidField(request, 0, "Bsb");
        }
    }

    [Theory]
    [InlineData("063-000", true)]
    [InlineData("063000", false)]
    public void TraceBsb_must_match_nnn_nnn_format(string traceBsb, bool expectedValid)
    {
        var request = ValidRequest(ValidInstruction() with { TraceBsb = traceBsb });

        if (expectedValid)
        {
            AssertValid(request);
        }
        else
        {
            AssertSingleInvalidField(request, 0, "TraceBsb");
        }
    }

    // --- AccountNumber / TraceAccountNumber ---

    [Theory]
    [InlineData("10001000", true)]
    [InlineData("123456789", true)] // exactly 9 chars, boundary
    [InlineData("1234567890", false)] // 10 chars, over max
    [InlineData("12345$678", false)] // disallowed character
    [InlineData("000000000", false)] // all zeros
    [InlineData("", false)] // all blank
    [InlineData("   ", false)] // all blank (spaces)
    public void AccountNumber_rules_are_enforced(string accountNumber, bool expectedValid)
    {
        var request = ValidRequest(ValidInstruction() with { AccountNumber = accountNumber });

        if (expectedValid)
        {
            AssertValid(request);
        }
        else
        {
            AssertSingleInvalidField(request, 0, "AccountNumber");
        }
    }

    [Theory]
    [InlineData("100000", true)]
    [InlineData("000000", false)]
    public void TraceAccountNumber_rules_are_enforced(string traceAccountNumber, bool expectedValid)
    {
        var request = ValidRequest(ValidInstruction() with { TraceAccountNumber = traceAccountNumber });

        if (expectedValid)
        {
            AssertValid(request);
        }
        else
        {
            AssertSingleInvalidField(request, 0, "TraceAccountNumber");
        }
    }

    // --- Indicator ---

    [Theory]
    [InlineData("N", true)]
    [InlineData("W", true)]
    [InlineData("X", true)]
    [InlineData("Y", true)]
    [InlineData("", true)]
    [InlineData("Z", false)]
    public void Indicator_must_be_a_supported_value_or_blank(string indicator, bool expectedValid)
    {
        var request = ValidRequest(ValidInstruction() with { Indicator = indicator });

        if (expectedValid)
        {
            AssertValid(request);
        }
        else
        {
            AssertSingleInvalidField(request, 0, "Indicator");
        }
    }

    // --- TransactionCode ---

    [Theory]
    [InlineData("13", true)]
    [InlineData("50", true)]
    [InlineData("51", true)]
    [InlineData("52", true)]
    [InlineData("53", true)]
    [InlineData("54", true)]
    [InlineData("55", true)]
    [InlineData("56", true)]
    [InlineData("57", true)]
    [InlineData("99", false)]
    public void TransactionCode_must_be_a_supported_code(string transactionCode, bool expectedValid)
    {
        var request = ValidRequest(ValidInstruction() with { TransactionCode = transactionCode });

        if (expectedValid)
        {
            AssertValid(request);
        }
        else
        {
            AssertSingleInvalidField(request, 0, "TransactionCode");
        }
    }

    // --- AmountInCents ---

    [Theory]
    [InlineData(0, true)]
    [InlineData(9_999_999_999, true)] // exactly 10 digits, boundary
    [InlineData(-1, false)]
    [InlineData(10_000_000_000, false)] // 11 digits, over max
    public void AmountInCents_must_be_non_negative_and_within_10_digits(long amountInCents, bool expectedValid)
    {
        var request = ValidRequest(ValidInstruction() with { AmountInCents = amountInCents });

        if (expectedValid)
        {
            AssertValid(request);
        }
        else
        {
            AssertSingleInvalidField(request, 0, "AmountInCents");
        }
    }

    // --- AccountTitle ---

    [Fact]
    public void AccountTitle_at_exactly_32_characters_is_valid() =>
        AssertValid(ValidRequest(ValidInstruction() with { AccountTitle = new string('A', 32) }));

    [Fact]
    public void AccountTitle_at_33_characters_is_invalid() =>
        AssertSingleInvalidField(
            ValidRequest(ValidInstruction() with { AccountTitle = new string('A', 33) }), 0, "AccountTitle");

    [Fact]
    public void AccountTitle_blank_is_invalid() =>
        AssertSingleInvalidField(ValidRequest(ValidInstruction() with { AccountTitle = "" }), 0, "AccountTitle");

    // --- LodgementReference ---

    [Fact]
    public void LodgementReference_blank_is_valid() =>
        AssertValid(ValidRequest(ValidInstruction() with { LodgementReference = "" }));

    [Fact]
    public void LodgementReference_at_exactly_18_characters_is_valid() =>
        AssertValid(ValidRequest(ValidInstruction() with { LodgementReference = new string('A', 18) }));

    [Fact]
    public void LodgementReference_at_19_characters_is_invalid() =>
        AssertSingleInvalidField(
            ValidRequest(ValidInstruction() with { LodgementReference = new string('A', 19) }),
            0,
            "LodgementReference");

    // --- RemitterName ---

    [Fact]
    public void RemitterName_at_exactly_16_characters_is_valid() =>
        AssertValid(ValidRequest(ValidInstruction() with { RemitterName = new string('A', 16) }));

    [Fact]
    public void RemitterName_at_17_characters_is_invalid() =>
        AssertSingleInvalidField(
            ValidRequest(ValidInstruction() with { RemitterName = new string('A', 17) }), 0, "RemitterName");

    [Fact]
    public void RemitterName_blank_is_invalid() =>
        AssertSingleInvalidField(ValidRequest(ValidInstruction() with { RemitterName = "" }), 0, "RemitterName");

    // --- WithholdingTaxAmountInCents ---

    [Theory]
    [InlineData(0, true)]
    [InlineData(99_999_999, true)] // exactly 8 digits, boundary
    [InlineData(-1, false)]
    [InlineData(100_000_000, false)] // 9 digits, over max
    public void WithholdingTaxAmountInCents_must_be_non_negative_and_within_8_digits(
        long withholdingTaxAmountInCents, bool expectedValid)
    {
        var request = ValidRequest(ValidInstruction() with { WithholdingTaxAmountInCents = withholdingTaxAmountInCents });

        if (expectedValid)
        {
            AssertValid(request);
        }
        else
        {
            AssertSingleInvalidField(request, 0, "WithholdingTaxAmountInCents");
        }
    }

    // --- Header-level: FileName / UserIdentificationNumber / DescriptionOfEntries ---

    [Fact]
    public void FileName_at_exactly_26_characters_is_valid() =>
        AssertValid(ValidRequest(ValidInstruction()) with { FileName = new string('A', 26) });

    [Fact]
    public void FileName_at_27_characters_is_invalid() =>
        AssertSingleInvalidField(
            ValidRequest(ValidInstruction()) with { FileName = new string('A', 27) }, -1, "FileName");

    [Fact]
    public void FileName_blank_is_invalid() =>
        AssertSingleInvalidField(ValidRequest(ValidInstruction()) with { FileName = "" }, -1, "FileName");

    [Fact]
    public void UserIdentificationNumber_at_exactly_6_digits_is_valid() =>
        AssertValid(ValidRequest(ValidInstruction()) with { UserIdentificationNumber = "123456" });

    [Fact]
    public void UserIdentificationNumber_at_7_digits_is_invalid() =>
        AssertSingleInvalidField(
            ValidRequest(ValidInstruction()) with { UserIdentificationNumber = "1234567" },
            -1,
            "UserIdentificationNumber");

    [Fact]
    public void UserIdentificationNumber_non_numeric_is_invalid() =>
        AssertSingleInvalidField(
            ValidRequest(ValidInstruction()) with { UserIdentificationNumber = "12A456" },
            -1,
            "UserIdentificationNumber");

    [Fact]
    public void DescriptionOfEntries_at_exactly_12_characters_is_valid() =>
        AssertValid(ValidRequest(ValidInstruction()) with { DescriptionOfEntries = new string('A', 12) });

    [Fact]
    public void DescriptionOfEntries_at_13_characters_is_invalid() =>
        AssertSingleInvalidField(
            ValidRequest(ValidInstruction()) with { DescriptionOfEntries = new string('A', 13) },
            -1,
            "DescriptionOfEntries");

    [Fact]
    public void DescriptionOfEntries_blank_is_invalid() =>
        AssertSingleInvalidField(
            ValidRequest(ValidInstruction()) with { DescriptionOfEntries = "" }, -1, "DescriptionOfEntries");

    // --- Minimum instruction count (F-008): spec requires at least 2 detail records ---

    private static ConvertDirectEntryBatchRequest RequestWithInstructionCount(int count) =>
        new(
            FileName: "COMPANY ABCD PTY LTD",
            UserIdentificationNumber: "301500",
            DescriptionOfEntries: "EFT-PAYMENT",
            DateToBeProcessed: new DateOnly(2026, 12, 5),
            Instructions: Enumerable.Repeat(ValidInstruction(), count).ToArray());

    [Fact]
    public void Zero_instructions_is_rejected_with_minimum_count_reason()
    {
        var errors = DirectEntryValidator.Validate(RequestWithInstructionCount(0));

        Assert.NotNull(errors);
        Assert.Contains(errors, e => e.Index == -1 && e.Reason.Contains("at least 2"));
    }

    [Fact]
    public void One_instruction_is_rejected_with_minimum_count_reason()
    {
        var errors = DirectEntryValidator.Validate(RequestWithInstructionCount(1));

        Assert.NotNull(errors);
        Assert.Contains(errors, e => e.Index == -1 && e.Reason.Contains("at least 2"));
    }

    [Fact]
    public void Two_instructions_are_not_rejected_on_minimum_count_ground() =>
        AssertValid(RequestWithInstructionCount(2)); // fully valid data otherwise -> no errors at all

    // --- Cross-cutting behaviour ---

    [Fact]
    public void Fully_valid_batch_proceeds_to_placeholder_conversion()
    {
        var command = new ConvertDirectEntryBatchCommand(ValidRequest(ValidInstruction()));

        var result = ConvertDirectEntryBatchHandler.Handle(command);

        Assert.True(result.Success);
        Assert.False(string.IsNullOrEmpty(result.ConvertedText));
        Assert.Null(result.Errors);
    }

    [Fact]
    public void Multiple_simultaneous_field_failures_on_one_instruction_are_all_reported()
    {
        var request = ValidRequest(ValidInstruction() with { Bsb = "bad", Indicator = "Z" });

        var errors = DirectEntryValidator.Validate(request);

        Assert.NotNull(errors);
        Assert.Equal(2, errors.Count);
        Assert.All(errors, e => Assert.Equal(0, e.Index));
        Assert.Contains(errors, e => e.Reason.Contains("Bsb"));
        Assert.Contains(errors, e => e.Reason.Contains("Indicator"));
    }

    [Fact]
    public void Header_level_failure_uses_index_negative_one()
    {
        var request = ValidRequest(ValidInstruction()) with { FileName = "" };

        var errors = DirectEntryValidator.Validate(request);

        Assert.NotNull(errors);
        Assert.All(errors, e => Assert.Equal(-1, e.Index));
    }

    [Fact]
    public void F004_router_rejection_short_circuits_F005_validation()
    {
        // Invalid payment type AND an invalid field on the same instruction: only the router's
        // error should surface — field validation must not run once routing has already rejected.
        var command = new ConvertDirectEntryBatchCommand(
            ValidRequest(ValidInstruction() with { PaymentType = "BPAY", Bsb = "bad" }));

        var result = ConvertDirectEntryBatchHandler.Handle(command);

        Assert.False(result.Success);
        Assert.Null(result.ConvertedText);
        Assert.NotNull(result.Errors);
        var error = Assert.Single(result.Errors);
        Assert.Equal(0, error.Index);
        Assert.Contains("Unsupported payment type", error.Reason);
    }
}
