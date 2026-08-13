using CommBiz.Api.Features.DirectEntry;

namespace CommBiz.Api.Tests;

public class DirectEntryDetailRecordMapperTests
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

    [Fact]
    public void Mapped_record_is_exactly_120_characters()
    {
        var record = DirectEntryDetailRecordMapper.Map(ValidInstruction());

        Assert.Equal(120, record.Length);
    }

    [Fact]
    public void Each_field_lands_at_its_documented_character_position()
    {
        var record = DirectEntryDetailRecordMapper.Map(ValidInstruction());

        Assert.Equal("1", record[0..1]); // Record Type, position 1, length 1
        Assert.Equal("062-000", record[1..8]); // BSB Number, position 2-8, length 7
        Assert.Equal("10001000", record[8..17].Trim()); // Account Number, position 9-17, length 9
        Assert.Equal("N", record[17..18]); // Indicator, position 18, length 1
        Assert.Equal("53", record[18..20]); // Transaction Code, position 19-20, length 2
        Assert.Equal("0000010050", record[20..30]); // Amount, position 21-30, length 10
        Assert.Equal("CLIENT COMPANY XYZ", record[30..62].TrimEnd()); // Title, position 31-62, length 32
        Assert.Equal("INVOICE 123456", record[62..80].TrimEnd()); // Lodgement Reference, position 63-80, length 18
        Assert.Equal("063-000", record[80..87]); // Trace BSB Number, position 81-87, length 7
        Assert.Equal("100000", record[87..96].Trim()); // Trace Account Number, position 88-96, length 9
        Assert.Equal("COMPANY ABCD P/L", record[96..112].TrimEnd()); // Name of Remitter, position 97-112, length 16
        Assert.Equal("00000000", record[112..120]); // Withholding Tax Amount, position 113-120, length 8
    }

    [Fact]
    public void Account_number_shorter_than_9_chars_is_right_justified_space_filled()
    {
        var record = DirectEntryDetailRecordMapper.Map(ValidInstruction() with { AccountNumber = "1000" });

        Assert.Equal("     1000", record[8..17]);
    }

    [Fact]
    public void Account_number_at_max_length_needs_no_padding()
    {
        var record = DirectEntryDetailRecordMapper.Map(ValidInstruction() with { AccountNumber = "123456789" });

        Assert.Equal("123456789", record[8..17]);
    }

    [Fact]
    public void Trace_account_number_shorter_than_9_chars_is_right_justified_space_filled()
    {
        var record = DirectEntryDetailRecordMapper.Map(ValidInstruction() with { TraceAccountNumber = "100000" });

        Assert.Equal("   100000", record[87..96]);
    }

    [Fact]
    public void Amount_requiring_full_10_digit_zero_fill_is_left_padded_with_zeros()
    {
        var record = DirectEntryDetailRecordMapper.Map(ValidInstruction() with { AmountInCents = 100 });

        Assert.Equal("0000000100", record[20..30]);
    }

    [Fact]
    public void Withholding_tax_amount_requiring_full_8_digit_zero_fill_is_left_padded_with_zeros()
    {
        var record = DirectEntryDetailRecordMapper.Map(
            ValidInstruction() with { WithholdingTaxAmountInCents = 100 });

        Assert.Equal("00000100", record[112..120]);
    }

    [Fact]
    public void Account_title_at_max_length_needs_no_padding()
    {
        var title = new string('A', 32);
        var record = DirectEntryDetailRecordMapper.Map(ValidInstruction() with { AccountTitle = title });

        Assert.Equal(title, record[30..62]);
    }

    [Fact]
    public void Account_title_shorter_than_max_is_left_justified_space_filled()
    {
        var record = DirectEntryDetailRecordMapper.Map(ValidInstruction() with { AccountTitle = "AB" });

        Assert.Equal("AB" + new string(' ', 30), record[30..62]);
    }

    [Fact]
    public void Lodgement_reference_shorter_than_max_is_left_justified_space_filled()
    {
        var record = DirectEntryDetailRecordMapper.Map(ValidInstruction() with { LodgementReference = "AB" });

        Assert.Equal("AB" + new string(' ', 16), record[62..80]);
    }

    [Fact]
    public void Remitter_name_shorter_than_max_is_left_justified_space_filled()
    {
        var record = DirectEntryDetailRecordMapper.Map(ValidInstruction() with { RemitterName = "AB" });

        Assert.Equal("AB" + new string(' ', 14), record[96..112]);
    }

    [Fact]
    public void Blank_indicator_maps_to_a_single_blank_character()
    {
        var record = DirectEntryDetailRecordMapper.Map(ValidInstruction() with { Indicator = "" });

        Assert.Equal(" ", record[17..18]);
    }
}
