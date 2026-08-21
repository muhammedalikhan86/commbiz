using CommBiz.Api.Features.BPay;

namespace CommBiz.Api.Tests.BPay;

public class BPayHeaderRecordMapperTests
{
    private static readonly BPaySettings Settings = new()
    {
        FundingAccount = "06200012345678",
        FileNumber = "001",
    };

    private static BPayPaymentInstructionRequest Instruction(DateTime paymentDate, decimal amount = 10000.00m) =>
        new(
            PaymentTypeCode: "BPAY",
            PaymentDate: paymentDate,
            Amount: amount,
            BPayBillerCode: "488577",
            BPayReference: "1202194308172118");

    private static IReadOnlyList<BPayPaymentInstructionRequest> ValidInstructions() =>
        [Instruction(new DateTime(2026, 8, 20, 10, 0, 0))];

    private static string[] Fields(string record) => record.Split(',');

    [Fact]
    public void Record_has_exactly_8_comma_separated_fields()
    {
        var record = BPayHeaderRecordMapper.Map(ValidInstructions(), Settings);

        Assert.Equal(8, Fields(record).Length);
    }

    [Fact]
    public void Record_type_is_literal_01()
    {
        var record = BPayHeaderRecordMapper.Map(ValidInstructions(), Settings);

        Assert.Equal("01", Fields(record)[0]);
    }

    [Fact]
    public void File_creation_date_and_time_are_the_current_utc_moment()
    {
        var before = DateTime.UtcNow;
        var record = BPayHeaderRecordMapper.Map(ValidInstructions(), Settings);
        var after = DateTime.UtcNow;

        var fields = Fields(record);
        var creationDate = DateTime.ParseExact(fields[1], "yyyyMMdd", null);
        Assert.InRange(creationDate, before.Date, after.Date);
        Assert.Equal(6, fields[2].Length);
        Assert.All(fields[2], c => Assert.True(char.IsDigit(c)));
    }

    [Fact]
    public void File_number_comes_from_settings()
    {
        var record = BPayHeaderRecordMapper.Map(ValidInstructions(), Settings with { FileNumber = "007" });

        Assert.Equal("007", Fields(record)[3]);
    }

    [Fact]
    public void Payment_account_comes_from_settings_funding_account()
    {
        var record = BPayHeaderRecordMapper.Map(ValidInstructions(), Settings);

        Assert.Equal("06200012345678", Fields(record)[4]);
    }

    [Fact]
    public void Payment_date_is_the_earliest_instruction_date_across_the_batch()
    {
        var instructions = new[]
        {
            Instruction(new DateTime(2026, 8, 20, 10, 0, 0)),
            Instruction(new DateTime(2026, 8, 14, 10, 0, 0)),
            Instruction(new DateTime(2026, 8, 25, 10, 0, 0)),
        };

        var record = BPayHeaderRecordMapper.Map(instructions, Settings);

        Assert.Equal("20260814", Fields(record)[5]);
    }

    [Fact]
    public void Number_of_payment_records_is_the_instruction_count()
    {
        var instructions = new[]
        {
            Instruction(new DateTime(2026, 8, 20, 10, 0, 0)),
            Instruction(new DateTime(2026, 8, 20, 10, 0, 0)),
        };

        var record = BPayHeaderRecordMapper.Map(instructions, Settings);

        Assert.Equal("2", Fields(record)[6]);
    }

    [Fact]
    public void Total_amount_is_the_sum_of_each_instruction_rounded_to_cents_first()
    {
        var instructions = new[]
        {
            Instruction(new DateTime(2026, 8, 20, 10, 0, 0), amount: 100.005m), // rounds to 100.01 -> 10001 cents
            Instruction(new DateTime(2026, 8, 20, 10, 0, 0), amount: 50.00m), // 5000 cents
        };

        var record = BPayHeaderRecordMapper.Map(instructions, Settings);

        Assert.Equal("15001", Fields(record)[7]);
    }

    [Fact]
    public void Sample_shaped_batch_matches_spec_sample_structure()
    {
        var instructions = new[]
        {
            Instruction(new DateTime(2026, 3, 8, 0, 0, 0), amount: 73.34m),
            Instruction(new DateTime(2026, 3, 8, 0, 0, 0), amount: 1095.89m),
        };

        var record = BPayHeaderRecordMapper.Map(instructions, Settings with { FundingAccount = "06200012345678" });
        var fields = Fields(record);

        Assert.Equal("01", fields[0]);
        Assert.Equal("001", fields[3]);
        Assert.Equal("06200012345678", fields[4]);
        Assert.Equal("20260308", fields[5]);
        Assert.Equal("2", fields[6]);
        Assert.Equal("116923", fields[7]);
    }

    [Fact]
    public void MapFields_returns_the_8_header_fields_in_spec_order()
    {
        var fields = BPayHeaderRecordMapper.MapFields(ValidInstructions(), Settings);

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
                "Total Amount of Payments",
            },
            fields.Select(field => field.CbaResponseField));
    }

    [Fact]
    public void MapFields_funding_account_and_file_number_are_attributed_to_the_static_appsettings_field_name()
    {
        var fields = BPayHeaderRecordMapper.MapFields(ValidInstructions(), Settings);

        Assert.Equal(nameof(BPaySettings.FundingAccount), fields.Single(f => f.CbaResponseField == "Payment Account").RequestField);
        Assert.Equal("06200012345678", fields.Single(f => f.CbaResponseField == "Payment Account").RequestValue);
        Assert.Equal(nameof(BPaySettings.FileNumber), fields.Single(f => f.CbaResponseField == "File Number").RequestField);
        Assert.Equal("001", fields.Single(f => f.CbaResponseField == "File Number").RequestValue);
    }

    [Fact]
    public void MapFields_payment_date_and_record_count_match_the_same_values_written_into_the_text_record()
    {
        var instructions = new[]
        {
            Instruction(new DateTime(2026, 8, 20, 10, 0, 0)),
            Instruction(new DateTime(2026, 8, 14, 10, 0, 0)),
        };
        var record = BPayHeaderRecordMapper.Map(instructions, Settings);
        var fields = BPayHeaderRecordMapper.MapFields(instructions, Settings);

        Assert.Equal(Fields(record)[5], fields.Single(f => f.CbaResponseField == "Payment Date").CbaResponseValue);
        Assert.Equal(Fields(record)[6], fields.Single(f => f.CbaResponseField == "Number of Payment Records").CbaResponseValue);
        Assert.Equal(Fields(record)[7], fields.Single(f => f.CbaResponseField == "Total Amount of Payments").CbaResponseValue);
    }

    // F-021 fix: Map and MapFields must share one caller-supplied `now`, not each independently call
    // DateTime.UtcNow, or the File Creation Date/Time in Mappings can drift from ConvertedText by a second.
    [Fact]
    public void Map_and_MapFields_given_the_same_explicit_now_produce_identical_file_creation_date_and_time()
    {
        var instructions = ValidInstructions();
        var now = new DateTime(2026, 8, 20, 23, 59, 59, DateTimeKind.Utc);

        var record = BPayHeaderRecordMapper.Map(instructions, Settings, now);
        var fields = BPayHeaderRecordMapper.MapFields(instructions, Settings, now);

        Assert.Equal(Fields(record)[1], fields.Single(f => f.CbaResponseField == "File Creation Date").CbaResponseValue);
        Assert.Equal(Fields(record)[2], fields.Single(f => f.CbaResponseField == "File Creation Time").CbaResponseValue);
    }
}

