using CommBiz.Api.Features.DirectEntry;

namespace CommBiz.Api.Tests;

public class DirectEntryTrailerRecordMapperTests
{
    private static PaymentInstructionRequest Instruction(string transactionCode, long amountInCents) =>
        new(
            PaymentType: "DirectEntry",
            Bsb: "062-000",
            AccountNumber: "10001000",
            Indicator: "N",
            TransactionCode: transactionCode,
            AmountInCents: amountInCents,
            AccountTitle: "CLIENT COMPANY XYZ",
            LodgementReference: "INVOICE 123456",
            TraceBsb: "063-000",
            TraceAccountNumber: "100000",
            RemitterName: "COMPANY ABCD P/L",
            WithholdingTaxAmountInCents: 0);

    [Fact]
    public void Mapped_record_is_exactly_120_characters()
    {
        var record = DirectEntryTrailerRecordMapper.Map([Instruction("53", 10050)]);

        Assert.Equal(120, record.Length);
    }

    [Fact]
    public void Each_field_lands_at_its_documented_character_position()
    {
        var record = DirectEntryTrailerRecordMapper.Map([Instruction("53", 10050)]);

        Assert.Equal("7", record[0..1]); // Record Type, position 1, length 1
        Assert.Equal("999-999", record[1..8]); // BSB Number, position 2-8, length 7
        Assert.Equal(new string(' ', 12), record[8..20]); // Blank, position 9-20, length 12
        Assert.Equal("0000010050", record[20..30]); // File Net Total Amount, position 21-30, length 10
        Assert.Equal("0000010050", record[30..40]); // File Credit Total Amount, position 31-40, length 10
        Assert.Equal("0000000000", record[40..50]); // File Debit Total Amount, position 41-50, length 10
        Assert.Equal(new string(' ', 24), record[50..74]); // Blank, position 51-74, length 24
        Assert.Equal("000001", record[74..80]); // File Count of Record Type 1, position 75-80, length 6
        Assert.Equal(new string(' ', 40), record[80..120]); // Blank, position 81-120, length 40
    }

    [Fact]
    public void All_credit_batch_totals_credit_only_and_net_equals_credit()
    {
        var record = DirectEntryTrailerRecordMapper.Map([Instruction("53", 10000), Instruction("50", 5000)]);

        Assert.Equal("0000015000", record[20..30]); // Net
        Assert.Equal("0000015000", record[30..40]); // Credit
        Assert.Equal("0000000000", record[40..50]); // Debit
    }

    [Fact]
    public void All_debit_batch_with_single_debit_instruction_totals_debit_only_and_net_equals_debit()
    {
        var record = DirectEntryTrailerRecordMapper.Map([Instruction("13", 7500)]);

        Assert.Equal("0000007500", record[20..30]); // Net
        Assert.Equal("0000000000", record[30..40]); // Credit
        Assert.Equal("0000007500", record[40..50]); // Debit
    }

    [Fact]
    public void Mixed_credit_and_debit_batch_nets_the_difference()
    {
        var record = DirectEntryTrailerRecordMapper.Map([Instruction("53", 10000), Instruction("13", 4000)]);

        Assert.Equal("0000006000", record[20..30]); // Net = |10000 - 4000|
        Assert.Equal("0000010000", record[30..40]); // Credit
        Assert.Equal("0000004000", record[40..50]); // Debit
    }

    [Fact]
    public void Net_total_is_unsigned_when_debit_total_exceeds_credit_total()
    {
        var record = DirectEntryTrailerRecordMapper.Map([Instruction("53", 1000), Instruction("13", 9000)]);

        Assert.Equal("0000008000", record[20..30]); // Net = |1000 - 9000|, not negative
        Assert.Equal("0000001000", record[30..40]); // Credit
        Assert.Equal("0000009000", record[40..50]); // Debit
    }

    [Fact]
    public void Record_count_matches_the_number_of_instructions()
    {
        var record = DirectEntryTrailerRecordMapper.Map(
            [Instruction("53", 100), Instruction("50", 200), Instruction("13", 50)]);

        Assert.Equal("000003", record[74..80]);
    }

    [Fact]
    public void Empty_instruction_list_produces_zeroed_totals_and_zero_count()
    {
        var record = DirectEntryTrailerRecordMapper.Map([]);

        Assert.Equal("0000000000", record[20..30]);
        Assert.Equal("0000000000", record[30..40]);
        Assert.Equal("0000000000", record[40..50]);
        Assert.Equal("000000", record[74..80]);
    }
}
