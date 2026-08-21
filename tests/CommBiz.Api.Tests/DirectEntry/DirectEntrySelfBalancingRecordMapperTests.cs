using CommBiz.Api.Features.DirectEntry;

namespace CommBiz.Api.Tests.DirectEntry;

public class DirectEntrySelfBalancingRecordMapperTests
{
    private static readonly DirectEntrySettings DebitSettings = new()
    {
        DescriptionOfEntriesOnFile = "ONLINEPAYMENTS",
        LodgementReferenceDetails = "PAYMENTS",
        TraceAccountBsb = "062-000",
        TraceAccountAccNo = "21120075",
        NameOfRemitter = "SHAW - AUD TRUST ACCOUNT",
        AmountOfWithholdingTax = "00000000",
        SelfBalancingAccountNo = "21120227",
        SelfBalancingNameOfRemitter = "SHAW AND PARTNER",
        SelfBalancingLodgementReferenceDetails = "SHAW CONSFEES",
    };

    private static PaymentInstructionRequest Instruction(decimal amount) =>
        new(
            PaymentTypeCode: "DE",
            AccountNo: "S1605677",
            SourceBankAccountNo: "111375004",
            SourceBankBsb: "015141",
            DestinationBankBsb: "484799",
            DestinationBankAccountNo: "300500",
            DestinationBankAccountName: "JOHN CITIZEN",
            PaymentDate: new DateTime(2026, 8, 20, 10, 0, 0),
            Amount: amount);

    [Fact]
    public void Mapped_record_is_exactly_120_characters()
    {
        var record = DirectEntrySelfBalancingRecordMapper.Map([Instruction(100.50m)], DebitSettings);

        Assert.Equal(120, record.Length);
    }

    [Fact]
    public void Each_field_lands_at_its_documented_character_position()
    {
        var record = DirectEntrySelfBalancingRecordMapper.Map([Instruction(100.50m)], DebitSettings);

        Assert.Equal("1", record[0..1]); // Record Type, position 1, length 1
        Assert.Equal("062-000", record[1..8]); // BSB Number = settlement account, position 2-8, length 7
        Assert.Equal("21120227", record[8..17].Trim()); // Account Number = settlement account, position 9-17, length 9
        Assert.Equal(" ", record[17..18]); // Indicator, position 18, length 1 - blank, not "N"
        Assert.Equal("13", record[18..20]); // Transaction Code, position 19-20, length 2 - inverse of debit "13"
        Assert.Equal("0000010050", record[20..30]); // Amount = batch total in cents, position 21-30, length 10
        Assert.Equal("SHAW AND PARTNERS LIMITED", record[30..62].TrimEnd()); // Title (mapper constant), position 31-62, length 32
        Assert.Equal("SHAW CONSFEES", record[62..80].TrimEnd()); // Lodgement Reference (self-balancing setting), position 63-80, length 18
        Assert.Equal("062-000", record[80..87]); // Trace BSB Number = settlement account, position 81-87, length 7
        Assert.Equal("21120227", record[87..96].Trim()); // Trace Account Number = settlement account, position 88-96, length 9
        Assert.Equal("SHAW AND PARTNER", record[96..112].TrimEnd()); // Name of Remitter (self-balancing setting), position 97-112, length 16
        Assert.Equal("00000000", record[112..120]); // Withholding Tax Amount, position 113-120, length 8
    }

    [Fact]
    public void Account_is_the_configured_settlement_account_not_any_instructions_account()
    {
        var record = DirectEntrySelfBalancingRecordMapper.Map(
            [Instruction(100.50m) with { SourceBankBsb = "111222", SourceBankAccountNo = "999999999" }],
            DebitSettings);

        Assert.Equal("062-000", record[1..8]);
        Assert.Equal("21120227", record[8..17].Trim());
    }

    [Fact]
    public void Trace_fields_always_match_the_settlement_account_regardless_of_any_instructions_destination()
    {
        var first = Instruction(100.00m) with
        {
            DestinationBankBsb = "111222",
            DestinationBankAccountNo = "999999",
            DestinationBankAccountName = "JANE DOE",
        };
        var second = Instruction(50.00m) with
        {
            DestinationBankBsb = "333444",
            DestinationBankAccountNo = "888888",
            DestinationBankAccountName = "OTHER PAYEE",
        };

        var record = DirectEntrySelfBalancingRecordMapper.Map([first, second], DebitSettings);

        Assert.Equal("062-000", record[80..87]);
        Assert.Equal("21120227", record[87..96].Trim());
        Assert.Equal("SHAW AND PARTNER", record[96..112].TrimEnd());
    }

    [Fact]
    public void Transaction_code_is_credit_when_configured_code_is_the_debit_code()
    {
        var record = DirectEntrySelfBalancingRecordMapper.Map([Instruction(100.00m)], DebitSettings);

        Assert.Equal("13", record[18..20]);
    }

    [Fact]
    public void Amount_equals_the_sum_of_all_instruction_amounts_in_cents()
    {
        var record = DirectEntrySelfBalancingRecordMapper.Map(
            [Instruction(100.00m), Instruction(50.00m), Instruction(0.50m)], DebitSettings);

        Assert.Equal("0000015050", record[20..30]);
    }

    [Fact]
    public void Amount_uses_the_shared_round_each_instruction_then_sum_computation_not_a_naive_sum_then_round()
    {
        // Each 0.005 individually rounds away-from-zero to 1 cent (2 cents total). A naive
        // Math.Round(sum * 100) would instead round the 0.01 dollar sum down to 1 cent - proving
        // this mapper shares DirectEntryAmountTotals with the trailer mapper rather than
        // recomputing the total independently (AC4 / PM-005).
        var record = DirectEntrySelfBalancingRecordMapper.Map(
            [Instruction(0.005m), Instruction(0.005m)], DebitSettings);

        Assert.Equal("0000000002", record[20..30]);
    }

    [Fact]
    public void MapFields_returns_the_same_12_spec_field_names_as_the_detail_mapper()
    {
        var fields = DirectEntrySelfBalancingRecordMapper.MapFields([Instruction(100.50m)], DebitSettings);

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
        var instructions = new[] { Instruction(100.00m), Instruction(50.00m) };
        var record = DirectEntrySelfBalancingRecordMapper.Map(instructions, DebitSettings);
        var fields = DirectEntrySelfBalancingRecordMapper.MapFields(instructions, DebitSettings);

        Assert.Equal(record[18..20], fields.Single(f => f.CbaResponseField == "Transaction Code").CbaResponseValue);
        Assert.Equal(record[20..30], fields.Single(f => f.CbaResponseField == "Amount").CbaResponseValue);
        Assert.Equal(record[1..8], fields.Single(f => f.CbaResponseField == "BSB Number").CbaResponseValue);
    }

    [Fact]
    public void MapFields_bsb_and_account_fields_are_attributed_to_the_settlement_account_config_field()
    {
        var fields = DirectEntrySelfBalancingRecordMapper.MapFields([Instruction(100.00m)], DebitSettings);

        Assert.Equal(
            nameof(DirectEntrySettings.TraceAccountBsb),
            fields.Single(f => f.CbaResponseField == "BSB Number").RequestField);
        Assert.Equal(
            nameof(DirectEntrySettings.SelfBalancingAccountNo),
            fields.Single(f => f.CbaResponseField == "Account Number to be Credited/Debited").RequestField);
    }
}
