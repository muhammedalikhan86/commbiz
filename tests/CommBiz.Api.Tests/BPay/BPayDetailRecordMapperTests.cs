using CommBiz.Api.Features.BPay;

namespace CommBiz.Api.Tests.BPay;

public class BPayDetailRecordMapperTests
{
    private static BPayPaymentInstructionRequest ValidInstruction() =>
        new(
            PaymentTypeCode: "BPAY",
            PaymentDate: DateTime.UtcNow.Date.AddDays(1),
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

    [Fact]
    public void MapFields_returns_all_25_fields_in_spec_order()
    {
        var fields = BPayDetailRecordMapper.MapFields(ValidInstruction());

        Assert.Equal(
            new[]
            {
                "Record Type",
                "File Creation Date",
                "File Creation Time",
                "File Number",
                "Payment Account",
                "Payment Date",
                "Number of Payment Records",
                "Currency Code of Payment",
                "Biller Code",
                "Service Code",
                "Customer Reference Number",
                "Payment Method",
                "Entry Method",
                "Amount",
                "Transaction Reference Number",
                "Original Reference Number",
                "BPAY Settlement Date",
                "Date Payment Accepted",
                "Time Payment Accepted",
                "Payer Name",
                "Additional Reference Code",
                "Error Correction Reason",
                "Discount Method",
                "Discount Reference",
                "Discretionary Data",
            },
            fields.Select(field => field.CbaResponseField));
    }

    [Fact]
    public void MapFields_previously_missing_unused_fields_appear_at_their_spec_position_with_empty_values()
    {
        var fields = BPayDetailRecordMapper.MapFields(ValidInstruction());

        var expected = new (int Index, string Name)[]
        {
            (1, "File Creation Date"),
            (2, "File Creation Time"),
            (3, "File Number"),
            (4, "Payment Account"),
            (5, "Payment Date"),
            (6, "Number of Payment Records"),
            (7, "Currency Code of Payment"),
            (9, "Service Code"),
            (11, "Payment Method"),
            (12, "Entry Method"),
            (14, "Transaction Reference Number"),
            (15, "Original Reference Number"),
            (16, "BPAY Settlement Date"),
            (17, "Date Payment Accepted"),
            (18, "Time Payment Accepted"),
            (19, "Payer Name"),
            (20, "Additional Reference Code"),
            (21, "Error Correction Reason"),
            (22, "Discount Method"),
            (23, "Discount Reference"),
            (24, "Discretionary Data"),
        };

        foreach (var (index, name) in expected)
        {
            Assert.Equal(name, fields[index].CbaResponseField);
            Assert.Equal("", fields[index].RequestField);
            Assert.Equal("", fields[index].RequestValue);
            Assert.Equal("", fields[index].CbaResponseValue);
        }
    }

    [Fact]
    public void MapFields_cba_response_values_match_the_same_values_written_into_the_text_record()
    {
        var instruction = ValidInstruction() with { BPayBillerCode = "7334", BPayReference = "8923037123", Amount = 1303.50m };
        var record = BPayDetailRecordMapper.Map(instruction);
        var fields = BPayDetailRecordMapper.MapFields(instruction);

        Assert.Equal(Fields(record)[8], fields.Single(f => f.CbaResponseField == "Biller Code").CbaResponseValue);
        Assert.Equal(Fields(record)[10], fields.Single(f => f.CbaResponseField == "Customer Reference Number").CbaResponseValue);
        Assert.Equal(Fields(record)[13], fields.Single(f => f.CbaResponseField == "Amount").CbaResponseValue);
    }
}
