using CommBiz.Api.Features.BPay;

namespace CommBiz.Api.Tests.BPay;

public class BPayDetailRecordMapperTests
{
    private static BPayPaymentInstructionRequest ValidInstruction() =>
        new(
            PaymentTypeCode: "BPAY",
            AccountNo: "S1218937",
            PaymentSourceTypeCode: "LEDGER",
            PaymentDate: new DateTime(2026, 8, 11, 10, 0, 0),
            Amount: 10000.00m,
            BPayBillerCode: "488577",
            BPayReference: "1202194308172118");

    private static string[] Fields(string record) => record.Split(',');

    [Fact]
    public void Record_has_exactly_25_comma_separated_fields()
    {
        var record = BPayDetailRecordMapper.Map(ValidInstruction());

        Assert.Equal(25, Fields(record).Length);
    }

    [Fact]
    public void Record_type_is_literal_50()
    {
        var record = BPayDetailRecordMapper.Map(ValidInstruction());

        Assert.Equal("50", Fields(record)[0]);
    }

    [Fact]
    public void Biller_code_lands_at_field_9()
    {
        var record = BPayDetailRecordMapper.Map(ValidInstruction() with { BPayBillerCode = "7334" });

        Assert.Equal("7334", Fields(record)[8]);
    }

    [Fact]
    public void Customer_reference_number_lands_at_field_11()
    {
        var record = BPayDetailRecordMapper.Map(ValidInstruction() with { BPayReference = "8923037123" });

        Assert.Equal("8923037123", Fields(record)[10]);
    }

    [Fact]
    public void Amount_in_cents_lands_at_field_14()
    {
        var record = BPayDetailRecordMapper.Map(ValidInstruction() with { Amount = 1303.50m });

        Assert.Equal("130350", Fields(record)[13]);
    }

    [Fact]
    public void Amount_with_fractional_cents_rounds_to_the_nearest_cent()
    {
        var record = BPayDetailRecordMapper.Map(ValidInstruction() with { Amount = 10.005m });

        Assert.Equal("1001", Fields(record)[13]); // 10.005 rounds away from zero to 10.01 -> 1001 cents
    }

    [Fact]
    public void All_fields_other_than_record_type_biller_reference_and_amount_are_empty()
    {
        var record = BPayDetailRecordMapper.Map(ValidInstruction());
        var fields = Fields(record);

        var mandatoryIndexes = new[] { 0, 8, 10, 13 };
        for (var index = 0; index < fields.Length; index++)
        {
            if (!mandatoryIndexes.Contains(index))
            {
                Assert.Equal(string.Empty, fields[index]);
            }
        }
    }

    [Fact]
    public void Record_ends_with_a_comma_since_field_25_discretionary_data_is_always_empty()
    {
        // Confirmed against the spec's own sample records (§1.2): "50,,,,,,,,7334,,8923037123,,,130350,,,,,,,,,,,"
        // ends in a comma too — field 25 being empty is what puts a trailing comma there, not a mapping bug.
        var record = BPayDetailRecordMapper.Map(ValidInstruction());

        Assert.EndsWith(",", record, StringComparison.Ordinal);
    }

    [Fact]
    public void Sample_shaped_instruction_matches_spec_sample_field_layout()
    {
        var instruction = ValidInstruction() with
        {
            BPayBillerCode = "7334",
            BPayReference = "8923037123",
            Amount = 1303.50m,
        };

        var record = BPayDetailRecordMapper.Map(instruction);

        Assert.Equal("50,,,,,,,,7334,,8923037123,,,130350,,,,,,,,,,,", record);
    }
}
