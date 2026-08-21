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
            Amount: 500.00m,
            BuyCurrency: "USD",
            SellCurrency: "AUD",
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
    public void Fields_8_through_11_13_through_20_and_23_through_27_are_empty_when_not_provided()
    {
        var record = FxRecordMapper.Map(ValidInstruction(), Settings);
        var fields = Fields(record);
        var emptyWhenNotProvidedIndexes = new[]
        {
            7, 8, 9, 10, // 8-11
            12, 13, 14, 15, 16, 17, 18, 19, // 13-20
            22, 23, 24, 25, 26, // 23-27
        };

        foreach (var index in emptyWhenNotProvidedIndexes)
        {
            Assert.Equal(string.Empty, fields[index]);
        }
    }

    [Fact]
    public void Fields_15_and_16_are_always_empty_even_when_a_beneficiary_is_populated()
    {
        var instruction = ValidInstruction() with
        {
            DestinationBankSwiftCode = "CITICNSXXXX",
            DestinationBankAccountName = "ABC Limited",
            BeneficiaryAddress = "1 Fifth Av",
        };

        var record = FxRecordMapper.Map(instruction, Settings);
        var fields = Fields(record);

        Assert.Equal(string.Empty, fields[14]); // 15: Beneficiary - Address line 2
        Assert.Equal(string.Empty, fields[15]); // 16: Beneficiary - Address line 3
    }

    [Fact]
    public void Field_8_and_9_map_Intermediary_bank_code_and_derived_country_when_present()
    {
        var instruction = ValidInstruction() with { IntermediaryBankSwiftCode = "CHASUS33" };

        var record = FxRecordMapper.Map(instruction, Settings);
        var fields = Fields(record);

        Assert.Equal("CHASUS33", fields[7]); // 8: Intermediary Bank - Bank Code
        Assert.Equal("US", fields[8]); // 9: Intermediary Institution - Country
    }

    [Fact]
    public void Field_10_11_and_20_map_Beneficiary_bank_code_and_derived_country_when_present()
    {
        var instruction = ValidInstruction() with { DestinationBankSwiftCode = "CITICNSXXXX" };

        var record = FxRecordMapper.Map(instruction, Settings);
        var fields = Fields(record);

        Assert.Equal("CITICNSXXXX", fields[9]); // 10: Beneficiary Bank - Bank Code
        Assert.Equal("CN", fields[10]); // 11: Beneficiary Bank - Country
        Assert.Equal("CN", fields[19]); // 20: Beneficiary - Country (same derivation as 11)
    }

    [Fact]
    public void Field_13_and_14_map_Beneficiary_account_name_and_address_when_present()
    {
        var instruction = ValidInstruction() with
        {
            DestinationBankAccountName = "ABC Limited",
            BeneficiaryAddress = "1 Fifth Av",
        };

        var record = FxRecordMapper.Map(instruction, Settings);
        var fields = Fields(record);

        Assert.Equal("ABC Limited", fields[12]); // 13: Beneficiary - Account Name
        Assert.Equal("1 Fifth Av", fields[13]); // 14: Beneficiary - Address line 1
    }

    [Fact]
    public void Field_8_and_10_are_truncated_to_11_characters()
    {
        var instruction = ValidInstruction() with
        {
            IntermediaryBankSwiftCode = "ABCDEFGHIJKLMNOP",
            DestinationBankSwiftCode = "ABCDEFGHIJKLMNOP",
        };

        var record = FxRecordMapper.Map(instruction, Settings);
        var fields = Fields(record);

        Assert.Equal("ABCDEFGHIJK", fields[7]); // 8
        Assert.Equal("ABCDEFGHIJK", fields[9]); // 10
    }

    [Fact]
    public void Field_13_is_truncated_to_62_characters()
    {
        var instruction = ValidInstruction() with { DestinationBankAccountName = new string('A', 70) };

        var record = FxRecordMapper.Map(instruction, Settings);

        Assert.Equal(new string('A', 62), Fields(record)[12]);
    }

    [Fact]
    public void Field_14_is_truncated_to_40_characters()
    {
        var instruction = ValidInstruction() with { BeneficiaryAddress = new string('A', 50) };

        var record = FxRecordMapper.Map(instruction, Settings);

        Assert.Equal(new string('A', 40), Fields(record)[13]);
    }

    [Fact]
    public void Fields_7_and_12_stay_the_configured_constant_even_when_a_beneficiary_is_present()
    {
        var instruction = ValidInstruction() with
        {
            DestinationBankSwiftCode = "CITICNSXXXX",
            DestinationBankAccountName = "ABC Limited",
        };

        var record = FxRecordMapper.Map(instruction, Settings);
        var fields = Fields(record);

        Assert.Equal("MAN", fields[6]); // 7: I SELL Instruction
        Assert.Equal("DOC", fields[11]); // 12: I BUY Instruction
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
            "Beneficiary - Address line 2",
            "Beneficiary - Address line 3",
            "Beneficiary - City/Suburb",
            "Beneficiary - State",
            "Beneficiary - Postcode",
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
    public void MapFields_beneficiary_bank_positions_have_empty_response_values_when_not_provided()
    {
        var fields = FxRecordMapper.MapFields(ValidInstruction(), Settings);

        var fieldNames = new[]
        {
            "Intermediary Bank - Bank Code",
            "Intermediary Institution - Country",
            "Beneficiary Bank - Bank Code",
            "Beneficiary Bank - Country",
            "Beneficiary - Account Name",
            "Beneficiary - Address line 1",
            "Beneficiary - Country",
        };

        foreach (var name in fieldNames)
        {
            var field = fields.Single(f => f.CbaResponseField == name);
            Assert.Null(field.RequestValue);
            Assert.Equal("", field.CbaResponseValue);
        }
    }

    [Fact]
    public void MapFields_beneficiary_bank_positions_carry_the_request_value_when_provided()
    {
        var instruction = ValidInstruction() with
        {
            IntermediaryBankSwiftCode = "CHASUS33",
            DestinationBankSwiftCode = "CITICNSXXXX",
            DestinationBankAccountName = "ABC Limited",
            BeneficiaryAddress = "1 Fifth Av",
        };

        var fields = FxRecordMapper.MapFields(instruction, Settings);

        Assert.Equal("US", fields.Single(f => f.CbaResponseField == "Intermediary Institution - Country").CbaResponseValue);
        Assert.Equal("CN", fields.Single(f => f.CbaResponseField == "Beneficiary Bank - Country").CbaResponseValue);
        Assert.Equal("CN", fields.Single(f => f.CbaResponseField == "Beneficiary - Country").CbaResponseValue);
        Assert.Equal("ABC Limited", fields.Single(f => f.CbaResponseField == "Beneficiary - Account Name").CbaResponseValue);
        Assert.Equal("1 Fifth Av", fields.Single(f => f.CbaResponseField == "Beneficiary - Address line 1").CbaResponseValue);
    }
}
