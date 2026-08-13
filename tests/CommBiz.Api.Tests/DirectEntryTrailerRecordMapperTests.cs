using CommBiz.Api.Features.DirectEntry;

namespace CommBiz.Api.Tests;

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
        Assert.Equal("0000010050", record[20..30]); // File Net Total Amount, position 21-30, length 10
        Assert.Equal("0000000000", record[30..40]); // File Credit Total Amount, position 31-40, length 10
        Assert.Equal("0000010050", record[40..50]); // File Debit Total Amount, position 41-50, length 10
        Assert.Equal(new string(' ', 24), record[50..74]); // Blank, position 51-74, length 24
        Assert.Equal("000001", record[74..80]); // File Count of Record Type 1, position 75-80, length 6
        Assert.Equal(new string(' ', 40), record[80..120]); // Blank, position 81-120, length 40
    }

    [Fact]
    public void Debit_batch_totals_debit_only_and_net_equals_debit()
    {
        var record = DirectEntryTrailerRecordMapper.Map(
            [Instruction(100.00m), Instruction(50.00m)], DebitSettings);

        Assert.Equal("0000015000", record[20..30]); // Net
        Assert.Equal("0000000000", record[30..40]); // Credit
        Assert.Equal("0000015000", record[40..50]); // Debit
    }

    [Fact]
    public void Credit_settings_totals_credit_only_and_net_equals_credit()
    {
        // A future payment type whose static TransactionCode isn't the debit code "13" would be
        // classified as a credit — the credit/debit distinction is kept general for that reason.
        var creditSettings = DebitSettings with { TransactionCode = "50" };

        var record = DirectEntryTrailerRecordMapper.Map(
            [Instruction(100.00m), Instruction(50.00m)], creditSettings);

        Assert.Equal("0000015000", record[20..30]); // Net
        Assert.Equal("0000015000", record[30..40]); // Credit
        Assert.Equal("0000000000", record[40..50]); // Debit
    }

    [Fact]
    public void Record_count_matches_the_number_of_instructions()
    {
        var record = DirectEntryTrailerRecordMapper.Map(
            [Instruction(1.00m), Instruction(2.00m), Instruction(0.50m)], DebitSettings);

        Assert.Equal("000003", record[74..80]);
    }

    [Fact]
    public void Empty_instruction_list_produces_zeroed_totals_and_zero_count()
    {
        var record = DirectEntryTrailerRecordMapper.Map([], DebitSettings);

        Assert.Equal("0000000000", record[20..30]);
        Assert.Equal("0000000000", record[30..40]);
        Assert.Equal("0000000000", record[40..50]);
        Assert.Equal("000000", record[74..80]);
    }
}

