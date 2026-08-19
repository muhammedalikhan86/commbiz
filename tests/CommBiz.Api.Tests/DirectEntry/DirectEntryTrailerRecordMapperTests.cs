using CommBiz.Api.Features.DirectEntry;

namespace CommBiz.Api.Tests.DirectEntry;

public class DirectEntryTrailerRecordMapperTests
{
    private static readonly DirectEntrySettings DebitSettings = new()
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
        TransactionCode = "13", // debit code — applies to every instruction for this payment type
    };

    private static PaymentInstructionRequest Instruction(decimal amount) =>
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
            Amount: amount,
            CreateBy: "James Harris");

    [Fact]
    public void Mapped_record_is_exactly_120_characters()
    {
        var record = DirectEntryTrailerRecordMapper.Map([Instruction(100.50m)], DebitSettings);

        Assert.Equal(120, record.Length);
    }

    [Fact]
    public void Each_field_lands_at_its_documented_character_position()
    {
        var record = DirectEntryTrailerRecordMapper.Map([Instruction(100.50m)], DebitSettings);

        Assert.Equal("7", record[0..1]); // Record Type, position 1, length 1
        Assert.Equal("999-999", record[1..8]); // BSB Number, position 2-8, length 7
        Assert.Equal(new string(' ', 12), record[8..20]); // Blank, position 9-20, length 12
        Assert.Equal("0000000000", record[20..30]); // File Net Total Amount, position 21-30, length 10
        Assert.Equal("0000010050", record[30..40]); // File Credit Total Amount, position 31-40, length 10
        Assert.Equal("0000010050", record[40..50]); // File Debit Total Amount, position 41-50, length 10
        Assert.Equal(new string(' ', 24), record[50..74]); // Blank, position 51-74, length 24
        Assert.Equal("000002", record[74..80]); // File Count of Record Type 1, position 75-80, length 6
        Assert.Equal(new string(' ', 40), record[80..120]); // Blank, position 81-120, length 40
    }

    [Fact]
    public void Debit_settings_credit_and_debit_totals_both_equal_the_batch_total_and_net_is_zero()
    {
        // The self-balancing (contra) record (F-014) always posts the batch total against the
        // opposite side of whichever direction is configured, so credit == debit == total, always.
        var record = DirectEntryTrailerRecordMapper.Map(
            [Instruction(100.00m), Instruction(50.00m)], DebitSettings);

        Assert.Equal("0000000000", record[20..30]); // Net
        Assert.Equal("0000015000", record[30..40]); // Credit
        Assert.Equal("0000015000", record[40..50]); // Debit
    }

    [Fact]
    public void Credit_settings_credit_and_debit_totals_both_equal_the_batch_total_and_net_is_zero()
    {
        // A future payment type whose static TransactionCode isn't the debit code "13" still yields
        // the same invariant - the self-balancing record's transaction code flips to match.
        var creditSettings = DebitSettings with { TransactionCode = "50" };

        var record = DirectEntryTrailerRecordMapper.Map(
            [Instruction(100.00m), Instruction(50.00m)], creditSettings);

        Assert.Equal("0000000000", record[20..30]); // Net
        Assert.Equal("0000015000", record[30..40]); // Credit
        Assert.Equal("0000015000", record[40..50]); // Debit
    }

    [Fact]
    public void Record_count_is_instruction_count_plus_one_for_the_self_balancing_record()
    {
        var record = DirectEntryTrailerRecordMapper.Map(
            [Instruction(1.00m), Instruction(2.00m), Instruction(0.50m)], DebitSettings);

        Assert.Equal("000004", record[74..80]);
    }

    [Fact]
    public void Empty_instruction_list_produces_zeroed_totals_but_a_count_of_one_for_the_self_balancing_record()
    {
        var record = DirectEntryTrailerRecordMapper.Map([], DebitSettings);

        Assert.Equal("0000000000", record[20..30]);
        Assert.Equal("0000000000", record[30..40]);
        Assert.Equal("0000000000", record[40..50]);
        Assert.Equal("000001", record[74..80]);
    }

    [Fact]
    public void MapFields_returns_all_9_trailer_fields_in_spec_order()
    {
        var fields = DirectEntryTrailerRecordMapper.MapFields([Instruction(100.50m)], DebitSettings);

        Assert.Equal(
            new[]
            {
                "Record Type",
                "BSB Number",
                "Blank",
                "File (User) Net Total Amount",
                "File (User) Credit Total Amount",
                "File (User) Debit Total Amount",
                "Blank",
                "File (User) Count of Record Type 1",
                "Blank",
            },
            fields.Select(field => field.CbaResponseField));
    }

    [Fact]
    public void MapFields_previously_missing_blank_filler_fields_hold_the_literal_spaces_value_at_their_spec_position()
    {
        var fields = DirectEntryTrailerRecordMapper.MapFields([Instruction(100.50m)], DebitSettings);

        Assert.Equal(new string(' ', 12), fields[2].CbaResponseValue);
        Assert.Equal(new string(' ', 24), fields[6].CbaResponseValue);
        Assert.Equal(new string(' ', 40), fields[8].CbaResponseValue);
    }

    [Fact]
    public void MapFields_cba_response_values_match_the_same_values_written_into_the_text_record()
    {
        var instructions = new[] { Instruction(100.00m), Instruction(50.00m) };
        var record = DirectEntryTrailerRecordMapper.Map(instructions, DebitSettings);
        var fields = DirectEntryTrailerRecordMapper.MapFields(instructions, DebitSettings);

        Assert.Equal(record[20..30], fields.Single(f => f.CbaResponseField == "File (User) Net Total Amount").CbaResponseValue);
        Assert.Equal(record[30..40], fields.Single(f => f.CbaResponseField == "File (User) Credit Total Amount").CbaResponseValue);
        Assert.Equal(record[40..50], fields.Single(f => f.CbaResponseField == "File (User) Debit Total Amount").CbaResponseValue);
        Assert.Equal(record[74..80], fields.Single(f => f.CbaResponseField == "File (User) Count of Record Type 1").CbaResponseValue);
    }
}

