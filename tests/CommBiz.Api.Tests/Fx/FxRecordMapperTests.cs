using CommBiz.Api.Features.Fx;

namespace CommBiz.Api.Tests.Fx;

public class FxRecordMapperTests
{
    private static readonly FxSettings Settings = new()
    {
        SellInstruction = "MAN",
        BuyInstruction = "DOC",
        BuyPaymentDetails = "Buy",
        SellPaymentDetails = "Sell",
    };

    // Confirmed sample shape, per the IPFX spec's Sample 2 ("New Settlement", USD/AUD, 500).
    private static FxPaymentInstructionRequest ValidInstruction() =>
        new(
            PaymentTypeCode: "FOREX",
            PaymentSourceTypeCode: "CMA",
            PaymentDate: new DateTime(2026, 4, 10, 10, 0, 0),
            Amount: 500.00m,
            Notes: "New Settlement",
            BuyCurrency: "USD",
            SellCurrency: "AUD",
            RateTypeCode: "SPOT",
            ValueDateTypeCode: "STANDARD",
            FeeTypeCode: "OUR",
            FeeOtherTypeCode: "",
            AccountNo: "Payment2");

    private static string[] Fields(string record) => record.Split(',');

    [Fact]
    public void Record_has_exactly_27_comma_separated_fields()
    {
        var record = FxRecordMapper.Map(ValidInstruction(), Settings);

        Assert.Equal(27, Fields(record).Length);
    }

    [Fact]
    public void Field_1_is_the_literal_constant_FX()
    {
        var record = FxRecordMapper.Map(ValidInstruction(), Settings);

        Assert.Equal("FX", Fields(record)[0]);
    }

    [Fact]
    public void Field_2_is_AccountNo()
    {
        var record = FxRecordMapper.Map(ValidInstruction(), Settings);

        Assert.Equal("Payment2", Fields(record)[1]);
    }

    [Fact]
    public void Field_3_is_BuyCurrency()
    {
        var record = FxRecordMapper.Map(ValidInstruction(), Settings);

        Assert.Equal("USD", Fields(record)[2]);
    }

    [Fact]
    public void Field_4_I_BUY_Amount_is_always_blank()
    {
        var record = FxRecordMapper.Map(ValidInstruction(), Settings);

        Assert.Equal(string.Empty, Fields(record)[3]);
    }

    [Fact]
    public void Field_5_is_SellCurrency()
    {
        var record = FxRecordMapper.Map(ValidInstruction(), Settings);

        Assert.Equal("AUD", Fields(record)[4]);
    }

    [Fact]
    public void Field_6_I_SELL_Amount_is_the_request_Amount()
    {
        var record = FxRecordMapper.Map(ValidInstruction(), Settings);

        Assert.Equal("500.00", Fields(record)[5]);
    }

    [Fact]
    public void Field_7_I_SELL_Instruction_is_the_static_configured_value()
    {
        var record = FxRecordMapper.Map(ValidInstruction(), Settings);

        Assert.Equal("MAN", Fields(record)[6]);
    }

    [Fact]
    public void Field_12_I_BUY_Instruction_is_the_static_configured_value()
    {
        var record = FxRecordMapper.Map(ValidInstruction(), Settings);

        Assert.Equal("DOC", Fields(record)[11]);
    }

    [Fact]
    public void Field_21_I_BUY_Payment_details_is_the_static_configured_value()
    {
        var record = FxRecordMapper.Map(ValidInstruction(), Settings);

        Assert.Equal("Buy", Fields(record)[20]);
    }

    [Fact]
    public void Field_22_I_SELL_Payment_details_is_the_static_configured_value()
    {
        var record = FxRecordMapper.Map(ValidInstruction(), Settings);

        Assert.Equal("Sell", Fields(record)[21]);
    }

    [Fact]
    public void Fields_8_through_11_13_through_20_and_23_through_27_are_always_empty()
    {
        var record = FxRecordMapper.Map(ValidInstruction(), Settings);
        var fields = Fields(record);
        var alwaysEmptyIndexes = new[]
        {
            7, 8, 9, 10, // 8-11
            12, 13, 14, 15, 16, 17, 18, 19, // 13-20
            22, 23, 24, 25, 26, // 23-27
        };

        foreach (var index in alwaysEmptyIndexes)
        {
            Assert.Equal(string.Empty, fields[index]);
        }
    }

    [Fact]
    public void MapFields_returns_exactly_27_entries()
    {
        var fields = FxRecordMapper.MapFields(ValidInstruction(), Settings);

        Assert.Equal(27, fields.Count);
    }

    [Fact]
    public void MapFields_returns_all_27_fields_in_spec_order()
    {
        var fields = FxRecordMapper.MapFields(ValidInstruction(), Settings);

        Assert.Equal(
            new[]
            {
                "Transaction Type",
                "Transaction Description",
                "I BUY Currency",
                "I BUY Amount",
                "I SELL Currency",
                "I SELL Amount",
                "I SELL Instruction",
                "Intermediary Bank - Bank Code",
                "Intermediary Institution - Country",
                "Beneficiary Bank - Bank Code",
                "Beneficiary Bank - Country",
                "I BUY Instruction",
                "Beneficiary - Account Name",
                "Beneficiary - Address line 1",
                "Beneficiary - Address line 2",
                "Beneficiary - Address line 3",
                "Beneficiary - City/Suburb",
                "Beneficiary - State",
                "Beneficiary - Postcode",
                "Beneficiary - Country",
                "I BUY Payment details",
                "I SELL Payment details",
                "Purpose of Payment",
                "CNAPS Code",
                "Beneficiary Company Name",
                "Beneficiary Contact Number",
                "Social Security Number (SSN)",
            },
            fields.Select(field => field.CbaResponseField));
    }

    [Fact]
    public void MapFields_blank_positions_have_empty_request_and_response_values()
    {
        var fields = FxRecordMapper.MapFields(ValidInstruction(), Settings);

        var blankFieldNames = new[]
        {
            "I BUY Amount",
            "Intermediary Bank - Bank Code",
            "Intermediary Institution - Country",
            "Beneficiary Bank - Bank Code",
            "Beneficiary Bank - Country",
            "Beneficiary - Account Name",
            "Beneficiary - Address line 1",
            "Beneficiary - Address line 2",
            "Beneficiary - Address line 3",
            "Beneficiary - City/Suburb",
            "Beneficiary - State",
            "Beneficiary - Postcode",
            "Beneficiary - Country",
            "Purpose of Payment",
            "CNAPS Code",
            "Beneficiary Company Name",
            "Beneficiary Contact Number",
            "Social Security Number (SSN)",
        };

        foreach (var name in blankFieldNames)
        {
            var field = fields.Single(f => f.CbaResponseField == name);
            Assert.Equal("", field.RequestField);
            Assert.Equal("", field.RequestValue);
            Assert.Equal("", field.CbaResponseValue);
        }
    }

    [Fact]
    public void MapFields_Notes_is_never_present_as_a_request_field()
    {
        var fields = FxRecordMapper.MapFields(ValidInstruction(), Settings);

        Assert.DoesNotContain(fields, f => f.RequestField == nameof(FxPaymentInstructionRequest.Notes));
    }
}
