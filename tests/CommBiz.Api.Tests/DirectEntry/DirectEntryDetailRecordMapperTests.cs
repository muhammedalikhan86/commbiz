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
            DestinationBankBsb: "484799",
            DestinationBankAccountNo: "300500",
            DestinationBankAccountName: "JOHN CITIZEN",
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
        Assert.Equal("062-000", record[1..8]); // BSB Number (Trace setting), position 2-8, length 7
        Assert.Equal("21120227", record[8..17].Trim()); // Account Number (Trace setting), position 9-17, length 9
        Assert.Equal("N", record[17..18]); // Indicator, position 18, length 1
        Assert.Equal("13", record[18..20]); // Transaction Code, position 19-20, length 2
        Assert.Equal("0000750000", record[20..30]); // Amount, position 21-30, length 10
        Assert.Equal("SHAW AND PARTNER", record[30..62].TrimEnd()); // Title (NameOfRemitter setting), position 31-62, length 32
        Assert.Equal("PAYMENTS", record[62..80].TrimEnd()); // Lodgement Reference, position 63-80, length 18
        Assert.Equal("484-799", record[80..87]); // Trace BSB Number (payload Destination), position 81-87, length 7
        Assert.Equal("300500", record[87..96].Trim()); // Trace Account Number (payload Destination), position 88-96, length 9
        Assert.Equal("JOHN CITIZEN", record[96..112].TrimEnd()); // Name of Remitter (payload Destination), position 97-112, length 16
        Assert.Equal("00000000", record[112..120]); // Withholding Tax Amount, position 113-120, length 8
    }

    [Fact]
    public void Bsb_number_and_account_number_always_come_from_trace_settings_regardless_of_instruction()
    {
        var record = DirectEntryDetailRecordMapper.Map(
            ValidInstruction() with { SourceBankBsb = "999999", SourceBankAccountNo = "999999999" }, Settings);

        Assert.Equal("062-000", record[1..8]);
        Assert.Equal("21120227", record[8..17].Trim());
    }

    [Fact]
    public void Account_number_setting_shorter_than_9_chars_is_right_justified_space_filled()
    {
        var record = DirectEntryDetailRecordMapper.Map(
            ValidInstruction(), Settings with { TraceAccountAccNo = "1000" });

        Assert.Equal("     1000", record[8..17]);
    }

    [Fact]
    public void Account_number_setting_at_max_length_needs_no_padding()
    {
        var record = DirectEntryDetailRecordMapper.Map(
            ValidInstruction(), Settings with { TraceAccountAccNo = "123456789" });

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
    public void Trace_bsb_is_reformatted_from_the_payloads_raw_6_digits_to_nnn_dash_nnn()
    {
        var record = DirectEntryDetailRecordMapper.Map(ValidInstruction() with { DestinationBankBsb = "063111" }, Settings);

        Assert.Equal("063-111", record[80..87]);
    }

    [Fact]
    public void Trace_account_number_shorter_than_9_chars_is_right_justified_space_filled()
    {
        var record = DirectEntryDetailRecordMapper.Map(
            ValidInstruction() with { DestinationBankAccountNo = "1000" }, Settings);

        Assert.Equal("     1000", record[87..96]);
    }

    [Fact]
    public void Trace_account_number_at_max_length_needs_no_padding()
    {
        var record = DirectEntryDetailRecordMapper.Map(
            ValidInstruction() with { DestinationBankAccountNo = "123456789" }, Settings);

        Assert.Equal("123456789", record[87..96]);
    }

    [Fact]
    public void Title_shorter_than_max_is_left_justified_space_filled()
    {
        var record = DirectEntryDetailRecordMapper.Map(ValidInstruction(), Settings with { NameOfRemitter = "AB" });

        Assert.Equal("AB" + new string(' ', 30), record[30..62]);
    }

    [Fact]
    public void Title_longer_than_max_is_truncated_to_keep_the_record_fixed_width()
    {
        var record = DirectEntryDetailRecordMapper.Map(
            ValidInstruction(), Settings with { NameOfRemitter = new string('A', 40) });

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
        var record = DirectEntryDetailRecordMapper.Map(
            ValidInstruction() with { DestinationBankAccountName = "AB" }, Settings);

        Assert.Equal("AB" + new string(' ', 14), record[96..112]);
    }

    [Fact]
    public void Remitter_name_longer_than_max_is_truncated_to_keep_the_record_fixed_width()
    {
        var record = DirectEntryDetailRecordMapper.Map(
            ValidInstruction() with { DestinationBankAccountName = new string('A', 20) }, Settings);

        Assert.Equal(new string('A', 16), record[96..112]);
        Assert.Equal(120, record.Length);
    }

    [Fact]
    public void MapFields_returns_the_12_populated_detail_fields_in_spec_order()
    {
        var fields = DirectEntryDetailRecordMapper.MapFields(ValidInstruction(), Settings);

        Assert.Equal(
            new[]
            {
                "Record Type",
                "BSB Number",
                "Account Number to be Credited/Debited",
                "Indicator",
                "Transaction Code",
                "Amount",
                "Title of Account to be Credited/Debited",
                "Lodgement Reference",
                "Trace BSB Number",
                "Trace Account Number",
                "Name of Remitter",
                "Amount of withholding tax",
            },
            fields.Select(field => field.CbaResponseField));
    }

    [Fact]
    public void MapFields_cba_response_values_match_the_same_values_written_into_the_text_record()
    {
        var record = DirectEntryDetailRecordMapper.Map(ValidInstruction(), Settings);
        var fields = DirectEntryDetailRecordMapper.MapFields(ValidInstruction(), Settings);

        Assert.Equal("062-000", fields.Single(f => f.CbaResponseField == "BSB Number").CbaResponseValue);
        Assert.Equal("484-799", fields.Single(f => f.CbaResponseField == "Trace BSB Number").CbaResponseValue);
        Assert.Equal("0000750000", fields.Single(f => f.CbaResponseField == "Amount").CbaResponseValue);
        Assert.Equal(record[112..120], fields.Single(f => f.CbaResponseField == "Amount of withholding tax").CbaResponseValue);
    }

    [Fact]
    public void MapFields_settings_sourced_bsb_number_field_is_attributed_to_the_trace_settings_field()
    {
        var fields = DirectEntryDetailRecordMapper.MapFields(ValidInstruction(), Settings);

        Assert.Equal("062-000", fields.Single(f => f.CbaResponseField == "BSB Number").RequestValue);
        Assert.Equal(
            nameof(DirectEntrySettings.TraceAccountBsb),
            fields.Single(f => f.CbaResponseField == "BSB Number").RequestField);
    }

    [Fact]
    public void MapFields_payload_sourced_trace_bsb_field_carries_the_raw_unformatted_request_value()
    {
        var fields = DirectEntryDetailRecordMapper.MapFields(ValidInstruction(), Settings);

        Assert.Equal("484799", fields.Single(f => f.CbaResponseField == "Trace BSB Number").RequestValue);
        Assert.Equal(
            nameof(PaymentInstructionRequest.DestinationBankBsb),
            fields.Single(f => f.CbaResponseField == "Trace BSB Number").RequestField);
    }
}

