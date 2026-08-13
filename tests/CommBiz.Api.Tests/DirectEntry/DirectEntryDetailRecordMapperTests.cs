using CommBiz.Api.Features.DirectEntry;

namespace CommBiz.Api.Tests.DirectEntry;

public class DirectEntryDetailRecordMapperTests
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

    [Fact]
    public void Mapped_record_is_exactly_120_characters()
    {
        var record = DirectEntryDetailRecordMapper.Map(ValidInstruction(), Settings);

        Assert.Equal(120, record.Length);
    }

    [Fact]
    public void Each_field_lands_at_its_documented_character_position()
    {
        var record = DirectEntryDetailRecordMapper.Map(ValidInstruction(), Settings);

        Assert.Equal("1", record[0..1]); // Record Type, position 1, length 1
        Assert.Equal("015-141", record[1..8]); // BSB Number, position 2-8, length 7
        Assert.Equal("111375004", record[8..17].Trim()); // Account Number, position 9-17, length 9
        Assert.Equal("N", record[17..18]); // Indicator, position 18, length 1
        Assert.Equal("13", record[18..20]); // Transaction Code, position 19-20, length 2
        Assert.Equal("0000750000", record[20..30]); // Amount, position 21-30, length 10
        Assert.Equal("SHAW AND PARTNERS LIMITED", record[30..62].TrimEnd()); // Title, position 31-62, length 32
        Assert.Equal("PAYMENTS", record[62..80].TrimEnd()); // Lodgement Reference, position 63-80, length 18
        Assert.Equal("062-000", record[80..87]); // Trace BSB Number, position 81-87, length 7
        Assert.Equal("21120227", record[87..96].Trim()); // Trace Account Number, position 88-96, length 9
        Assert.Equal("SHAW AND PARTNER", record[96..112].TrimEnd()); // Name of Remitter, position 97-112, length 16
        Assert.Equal("00000000", record[112..120]); // Withholding Tax Amount, position 113-120, length 8
    }

    [Fact]
    public void Bsb_is_reformatted_from_raw_6_digits_to_nnn_dash_nnn()
    {
        var record = DirectEntryDetailRecordMapper.Map(ValidInstruction() with { SourceBankBsb = "063111" }, Settings);

        Assert.Equal("063-111", record[1..8]);
    }

    [Fact]
    public void Account_number_shorter_than_9_chars_is_right_justified_space_filled()
    {
        var record = DirectEntryDetailRecordMapper.Map(
            ValidInstruction() with { SourceBankAccountNo = "1000" }, Settings);

        Assert.Equal("     1000", record[8..17]);
    }

    [Fact]
    public void Account_number_at_max_length_needs_no_padding()
    {
        var record = DirectEntryDetailRecordMapper.Map(
            ValidInstruction() with { SourceBankAccountNo = "123456789" }, Settings);

        Assert.Equal("123456789", record[8..17]);
    }

    [Fact]
    public void Amount_is_converted_to_cents_and_zero_filled_to_10_digits()
    {
        var record = DirectEntryDetailRecordMapper.Map(ValidInstruction() with { Amount = 1.00m }, Settings);

        Assert.Equal("0000000100", record[20..30]);
    }

    [Fact]
    public void Amount_with_fractional_cents_rounds_to_the_nearest_cent()
    {
        var record = DirectEntryDetailRecordMapper.Map(ValidInstruction() with { Amount = 10.005m }, Settings);

        Assert.Equal("0000001001", record[20..30]); // 10.005 rounds away from zero to 10.01 -> 1001 cents
    }

    [Fact]
    public void Transaction_code_always_comes_from_settings_regardless_of_instruction()
    {
        var record = DirectEntryDetailRecordMapper.Map(
            ValidInstruction(), Settings with { TransactionCode = "50" });

        Assert.Equal("50", record[18..20]);
    }

    [Fact]
    public void Indicator_is_always_the_literal_N()
    {
        var record = DirectEntryDetailRecordMapper.Map(ValidInstruction(), Settings);

        Assert.Equal("N", record[17..18]);
    }

    [Fact]
    public void Trace_account_number_shorter_than_9_chars_is_right_justified_space_filled()
    {
        var record = DirectEntryDetailRecordMapper.Map(
            ValidInstruction(), Settings with { TraceAccountAccNo = "100000" });

        Assert.Equal("   100000", record[87..96]);
    }

    [Fact]
    public void Title_shorter_than_max_is_left_justified_space_filled()
    {
        var record = DirectEntryDetailRecordMapper.Map(ValidInstruction(), Settings with { Title = "AB" });

        Assert.Equal("AB" + new string(' ', 30), record[30..62]);
    }

    [Fact]
    public void Title_longer_than_max_is_truncated_to_keep_the_record_fixed_width()
    {
        var record = DirectEntryDetailRecordMapper.Map(
            ValidInstruction(), Settings with { Title = new string('A', 40) });

        Assert.Equal(new string('A', 32), record[30..62]);
        Assert.Equal(120, record.Length);
    }

    [Fact]
    public void Lodgement_reference_shorter_than_max_is_left_justified_space_filled()
    {
        var record = DirectEntryDetailRecordMapper.Map(
            ValidInstruction(), Settings with { LodgementReferenceDetails = "AB" });

        Assert.Equal("AB" + new string(' ', 16), record[62..80]);
    }

    [Fact]
    public void Remitter_name_shorter_than_max_is_left_justified_space_filled()
    {
        var record = DirectEntryDetailRecordMapper.Map(ValidInstruction(), Settings with { NameOfRemitter = "AB" });

        Assert.Equal("AB" + new string(' ', 14), record[96..112]);
    }
}

