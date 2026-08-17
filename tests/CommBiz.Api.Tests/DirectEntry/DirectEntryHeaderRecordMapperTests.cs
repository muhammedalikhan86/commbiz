using CommBiz.Api.Features.DirectEntry;

namespace CommBiz.Api.Tests.DirectEntry;

public class DirectEntryHeaderRecordMapperTests
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

    private static PaymentInstructionRequest Instruction(DateTime paymentDate) =>
        new(
            PaymentTypeCode: "DE",
            AccountNo: "S1605677",
            PaymentSourceTypeCode: "CMA",
            SourceBankAccountName: "SOPHIA CLARK",
            SourceBankAccountNo: "111375004",
            SourceBankBsb: "015141",
            PaymentDate: paymentDate,
            SourceCurrency: "AUD",
            SourceAmount: 0.0m,
            Amount: 7500.0m,
            CreateBy: "James Harris");

    private static IReadOnlyList<PaymentInstructionRequest> ValidInstructions() =>
        [Instruction(new DateTime(2026, 12, 5, 10, 0, 0))];

    [Fact]
    public void Mapped_record_is_exactly_120_characters()
    {
        var record = DirectEntryHeaderRecordMapper.Map(ValidInstructions(), Settings);

        Assert.Equal(120, record.Length);
    }

    [Fact]
    public void Each_field_lands_at_its_documented_character_position()
    {
        var record = DirectEntryHeaderRecordMapper.Map(ValidInstructions(), Settings);

        Assert.Equal("0", record[0..1]); // Record Type, position 1, length 1
        Assert.Equal(new string(' ', 17), record[1..18]); // Blank, position 2-18, length 17
        Assert.Equal("01", record[18..20]); // Reel Sequence Number, position 19-20, length 2
        Assert.Equal("CBA", record[20..23]); // Name of User Financial Institution, position 21-23, length 3
        Assert.Equal(new string(' ', 7), record[23..30]); // Blank, position 24-30, length 7
        // Name of User Supplying File, position 31-56, length 26
        Assert.Equal("SHAW AND PARTNERS LIMITED", record[30..56].TrimEnd());
        Assert.Equal("301500", record[56..62]); // User Identification Number, position 57-62, length 6
        // Description of Entries on File is 14 chars but the field is only 12 wide — truncated (see Assumptions).
        Assert.Equal("ONLINEPAYMEN", record[62..74]); // Description of Entries, position 63-74, length 12
        Assert.Equal("051226", record[74..80]); // Date to be Processed, position 75-80, length 6
        Assert.Equal(new string(' ', 40), record[80..120]); // Blank, position 81-120, length 40
    }

    [Fact]
    public void Name_of_user_supplying_file_shorter_than_max_is_left_justified_space_filled()
    {
        var record = DirectEntryHeaderRecordMapper.Map(
            ValidInstructions(), Settings with { NameOfUserSupplyingFile = "AB" });

        Assert.Equal("AB" + new string(' ', 24), record[30..56]);
    }

    [Fact]
    public void Name_of_user_supplying_file_at_max_length_needs_no_padding()
    {
        var name = new string('A', 26);
        var record = DirectEntryHeaderRecordMapper.Map(
            ValidInstructions(), Settings with { NameOfUserSupplyingFile = name });

        Assert.Equal(name, record[30..56]);
    }

    [Fact]
    public void User_identification_number_shorter_than_6_digits_is_right_justified_zero_filled()
    {
        var record = DirectEntryHeaderRecordMapper.Map(
            ValidInstructions(), Settings with { UserIdentificationNumber = "42" });

        Assert.Equal("000042", record[56..62]);
    }

    [Fact]
    public void Missing_user_identification_number_zero_fills_to_all_zeros()
    {
        var record = DirectEntryHeaderRecordMapper.Map(
            ValidInstructions(), Settings with { UserIdentificationNumber = "" });

        Assert.Equal("000000", record[56..62]);
    }

    [Fact]
    public void Description_of_entries_shorter_than_max_is_left_justified_space_filled()
    {
        var record = DirectEntryHeaderRecordMapper.Map(
            ValidInstructions(), Settings with { DescriptionOfEntriesOnFile = "AB" });

        Assert.Equal("AB" + new string(' ', 10), record[62..74]);
    }

    [Fact]
    public void Date_with_single_digit_day_and_month_formats_as_ddmmyy_without_dropping_leading_zeros()
    {
        var record = DirectEntryHeaderRecordMapper.Map(
            [Instruction(new DateTime(2025, 1, 5, 10, 0, 0))], Settings);

        Assert.Equal("050125", record[74..80]);
    }

    [Fact]
    public void Header_date_is_the_earliest_payment_date_across_the_batch()
    {
        var instructions = new[]
        {
            Instruction(new DateTime(2026, 8, 20, 10, 0, 0)),
            Instruction(new DateTime(2026, 8, 14, 10, 0, 0)),
            Instruction(new DateTime(2026, 8, 25, 10, 0, 0)),
        };

        var record = DirectEntryHeaderRecordMapper.Map(instructions, Settings);

        Assert.Equal("140826", record[74..80]);
    }

    [Fact]
    public void MapFields_returns_all_10_header_fields_in_spec_order()
    {
        var fields = DirectEntryHeaderRecordMapper.MapFields(ValidInstructions(), Settings);

        Assert.Equal(
            new[]
            {
                "Record Type",
                "Blank",
                "Reel Sequence Number",
                "Name Of User Financial Institution",
                "Blank",
                "Name of User Supplying File",
                "Number of User Supplying File",
                "Description of Entries on File",
                "Date to be Processed",
                "Blank",
            },
            fields.Select(field => field.CbaResponseField));
    }

    [Fact]
    public void MapFields_previously_missing_blank_filler_fields_hold_the_literal_spaces_value_at_their_spec_position()
    {
        var fields = DirectEntryHeaderRecordMapper.MapFields(ValidInstructions(), Settings);

        Assert.Equal(new string(' ', 17), fields[1].CbaResponseValue);
        Assert.Equal(new string(' ', 7), fields[4].CbaResponseValue);
        Assert.Equal(new string(' ', 40), fields[9].CbaResponseValue);
    }

    [Fact]
    public void MapFields_cba_response_values_match_the_same_values_written_into_the_text_record()
    {
        var record = DirectEntryHeaderRecordMapper.Map(ValidInstructions(), Settings);
        var fields = DirectEntryHeaderRecordMapper.MapFields(ValidInstructions(), Settings);

        Assert.Equal("0", fields.Single(f => f.CbaResponseField == "Record Type").CbaResponseValue);
        Assert.Equal("CBA", fields.Single(f => f.CbaResponseField == "Name Of User Financial Institution").CbaResponseValue);
        Assert.Equal(
            record[30..56].TrimEnd(),
            fields.Single(f => f.CbaResponseField == "Name of User Supplying File").CbaResponseValue!.TrimEnd());
        Assert.Equal("301500", fields.Single(f => f.CbaResponseField == "Number of User Supplying File").CbaResponseValue);
        Assert.Equal("051226", fields.Single(f => f.CbaResponseField == "Date to be Processed").CbaResponseValue);
    }

    [Fact]
    public void MapFields_config_sourced_fields_are_attributed_to_the_settings_field_name_not_the_request()
    {
        var fields = DirectEntryHeaderRecordMapper.MapFields(ValidInstructions(), Settings);

        Assert.Equal(nameof(DirectEntrySettings.InstitutionCode), fields.Single(f => f.CbaResponseField == "Name Of User Financial Institution").RequestField);
        Assert.Equal(nameof(DirectEntrySettings.UserIdentificationNumber), fields.Single(f => f.CbaResponseField == "Number of User Supplying File").RequestField);
    }
}

