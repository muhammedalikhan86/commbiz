using CommBiz.Api.Features.DirectEntry;

namespace CommBiz.Api.Tests.DirectEntry;

public class DirectEntrySelfBalancingRecordMapperTests
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
        TransactionCode = "13", // debit code
    };

    private static PaymentInstructionRequest Instruction(decimal amount) =>
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
            Amount: amount,
            CreateBy: "James Harris");

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
        Assert.Equal("N", record[17..18]); // Indicator, position 18, length 1
        Assert.Equal("50", record[18..20]); // Transaction Code, position 19-20, length 2 - inverse of debit "13"
        Assert.Equal("0000010050", record[20..30]); // Amount = batch total in cents, position 21-30, length 10
        Assert.Equal("SHAW AND PARTNERS LIMITED", record[30..62].TrimEnd()); // Title, position 31-62, length 32
        Assert.Equal("PAYMENTS", record[62..80].TrimEnd()); // Lodgement Reference, position 63-80, length 18
        Assert.Equal("062-000", record[80..87]); // Trace BSB Number, position 81-87, length 7
        Assert.Equal("21120227", record[87..96].Trim()); // Trace Account Number, position 88-96, length 9
        Assert.Equal("SHAW AND PARTNER", record[96..112].TrimEnd()); // Name of Remitter, position 97-112, length 16
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
    public void Transaction_code_is_credit_when_configured_code_is_the_debit_code()
    {
        var record = DirectEntrySelfBalancingRecordMapper.Map([Instruction(100.00m)], DebitSettings);

        Assert.Equal("50", record[18..20]);
    }

    [Fact]
    public void Transaction_code_is_debit_when_configured_code_is_a_credit_code()
    {
        var creditSettings = DebitSettings with { TransactionCode = "50" };

        var record = DirectEntrySelfBalancingRecordMapper.Map([Instruction(100.00m)], creditSettings);

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
    public void Title_lodgement_reference_trace_account_remitter_and_withholding_tax_match_the_detail_mapper()
    {
        var instructions = new[] { Instruction(100.00m) };
        var detailRecord = DirectEntryDetailRecordMapper.Map(instructions[0], DebitSettings);
        var selfBalancingRecord = DirectEntrySelfBalancingRecordMapper.Map(instructions, DebitSettings);

        Assert.Equal(detailRecord[30..62], selfBalancingRecord[30..62]); // Title
        Assert.Equal(detailRecord[62..80], selfBalancingRecord[62..80]); // Lodgement Reference
        Assert.Equal(detailRecord[80..96], selfBalancingRecord[80..96]); // Trace BSB/Account
        Assert.Equal(detailRecord[96..112], selfBalancingRecord[96..112]); // Name of Remitter
        Assert.Equal(detailRecord[112..120], selfBalancingRecord[112..120]); // Withholding Tax
    }
}
