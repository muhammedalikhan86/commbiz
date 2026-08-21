using CommBiz.Api.Features.Imt;

namespace CommBiz.Api.Tests.Imt;

public class ImtRecordMapperTests
{
    private static readonly ImtSettings Settings = new()
    {
        DebitAccountBsb = "062-000",
        DebitAccountNumber = "2112 0075",
        DebitAccountName = "SHAW - AUD TRUST ACCOUNT",
    };

    private static ImtPaymentInstructionRequest ValidInstruction() =>
        new(
            PaymentTypeCode: "TT",
            SourceBankAccountName: null,
            SourceBankAccountNo: null,
            SourceBankBsb: null,
            DestinationBankTypeCode: null,
            DestinationBankAccountName: "SAMER MOHAMMED KIKI",
            DestinationBankAccountNo: "658450191",
            PaymentDate: new DateTime(2026, 4, 23, 10, 0, 0),
            SourceCurrency: "USD",
            SourceAmount: 588517.58m,
            Amount: 0.0m,
            PaymentReference: "TT-000001",
            Notes: "10 bps on fx",
            Currency: null,
            DestinationBankIBAN: null,
            DestinationBankSwiftCode: "CHASUS33",
            DestinationBankName: "NATIONAL FINANCIAL SERVICES",
            DestinationBankAddress: "640 5TH AVENUE NEW YORK NY 10019",
            BeneficiaryAddress: "9101 Alta Drive, U15, Las Vegas, NV 89145",
            IntermediaryBankIBAN: null,
            IntermediaryBankSwiftCode: "CHASUS33",
            IntermediaryBankName: "CHASE MANHATTAN BANK (J.P. MORGAN CHASE & CO)",
            IntermediaryBankAddress: "1 CHASE MANHATTAN PLAZA NEW YORK NY 10005");

    private static string[] Fields(string record) => record.Split(',');

    private static string DebitAccountNumber => ImtRecordMapper.DeriveDebitAccountNumber(Settings);

    [Fact]
    public void Record_has_exactly_27_comma_separated_fields()
    {
        var record = ImtRecordMapper.Map(ValidInstruction(), DebitAccountNumber);

        Assert.Equal(27, Fields(record).Length);
    }

    [Fact]
    public void Field_1_is_the_literal_constant_IMT_never_TT()
    {
        var record = ImtRecordMapper.Map(ValidInstruction(), DebitAccountNumber);

        Assert.Equal("IMT", Fields(record)[0]);
    }

    [Fact]
    public void Field_2_Notes_at_or_under_12_characters_is_not_truncated()
    {
        var record = ImtRecordMapper.Map(ValidInstruction(), DebitAccountNumber);

        Assert.Equal("10 bps on fx", Fields(record)[1]);
    }

    [Fact]
    public void Field_2_Notes_over_12_characters_is_truncated()
    {
        var instruction = ValidInstruction() with { Notes = "Invoice payment for August services" };
        var record = ImtRecordMapper.Map(instruction, DebitAccountNumber);

        Assert.Equal("Invoice paym", Fields(record)[1]);
    }

    [Fact]
    public void MapFields_Transaction_Description_keeps_the_untruncated_request_value_but_truncates_the_response_value()
    {
        var instruction = ValidInstruction() with { Notes = "Invoice payment for August services" };

        var fields = ImtRecordMapper.MapFields(instruction, DebitAccountNumber);
        var field = fields.Single(f => f.CbaResponseField == "Transaction Description");

        Assert.Equal("Invoice payment for August services", field.RequestValue);
        Assert.Equal("Invoice paym", field.CbaResponseValue);
    }

    [Fact]
    public void Field_3_process_date_is_formatted_YYMMDD()
    {
        var record = ImtRecordMapper.Map(ValidInstruction(), DebitAccountNumber);

        Assert.Equal("260423", Fields(record)[2]);
    }

    [Fact]
    public void Field_5_payment_amount_is_populated_when_source_amount_is_positive_and_field_6_is_blank()
    {
        var record = ImtRecordMapper.Map(ValidInstruction(), DebitAccountNumber);
        var fields = Fields(record);

        Assert.Equal("588517.58", fields[4]);
        Assert.Equal(string.Empty, fields[5]);
    }

    [Fact]
    public void Field_6_debit_amount_is_populated_when_amount_is_positive_and_field_5_is_blank()
    {
        var instruction = ValidInstruction() with { SourceAmount = 0m, Amount = 1234.56m };
        var record = ImtRecordMapper.Map(instruction, DebitAccountNumber);
        var fields = Fields(record);

        Assert.Equal(string.Empty, fields[4]);
        Assert.Equal("1234.56", fields[5]);
    }

    [Fact]
    public void Field_7_debit_account_number_is_derived_from_static_ImtSettings()
    {
        var record = ImtRecordMapper.Map(ValidInstruction(), DebitAccountNumber);

        // BSB "062-000" -> last 4 digits "2000"; account number "2112 0075" -> spaces stripped "21120075"
        Assert.Equal("200021120075", Fields(record)[6]);
    }

    [Fact]
    public void Field_13_intermediary_country_is_derived_from_SWIFT_chars_5_and_6()
    {
        var record = ImtRecordMapper.Map(ValidInstruction(), DebitAccountNumber);

        Assert.Equal("US", Fields(record)[12]);
    }

    [Fact]
    public void Field_17_beneficiary_bank_country_is_derived_from_SWIFT_chars_5_and_6()
    {
        var record = ImtRecordMapper.Map(ValidInstruction(), DebitAccountNumber);

        Assert.Equal("US", Fields(record)[16]);
    }

    [Fact]
    public void Field_26_beneficiary_country_matches_field_17_derivation()
    {
        var record = ImtRecordMapper.Map(ValidInstruction(), DebitAccountNumber);
        var fields = Fields(record);

        Assert.Equal(fields[16], fields[25]);
    }

    [Fact]
    public void Field_20_beneficiary_address_commas_are_sanitized_to_spaces()
    {
        var record = ImtRecordMapper.Map(ValidInstruction(), DebitAccountNumber);

        Assert.Equal("9101 Alta Drive U15 Las Vegas NV 89145", Fields(record)[19]);
    }

    [Fact]
    public void Fields_8_9_12_16_21_22_23_24_25_are_always_empty()
    {
        var record = ImtRecordMapper.Map(ValidInstruction(), DebitAccountNumber);
        var fields = Fields(record);
        var alwaysEmptyIndexes = new[] { 7, 8, 11, 15, 20, 21, 22, 23, 24 };

        foreach (var index in alwaysEmptyIndexes)
        {
            Assert.Equal(string.Empty, fields[index]);
        }
    }

    [Fact]
    public void No_field_is_padded_or_fixed_width()
    {
        var record = ImtRecordMapper.Map(ValidInstruction() with { SourceCurrency = "USD" }, DebitAccountNumber);

        Assert.StartsWith("IMT,10 bps on fx,260423,USD,", record, StringComparison.Ordinal);
    }

    [Fact]
    public void MapFields_returns_all_27_fields_in_spec_order()
    {
        var fields = ImtRecordMapper.MapFields(ValidInstruction(), DebitAccountNumber);

        Assert.Equal(
            new[]
            {
                "Transaction Type",
                "Transaction Description",
                "Process Date",
                "Payment Currency",
                "Payment Amount",
                "Debit Amount",
                "Debit Account - Account Number",
                "Dealer Code",
                "Dealer Exchange Rate",
                "Intermediary Bank - Bank Code",
                "Intermediary Bank - Name",
                "Intermediary Bank - City",
                "Intermediary Institution - Country",
                "Beneficiary Bank - Bank Code",
                "Beneficiary Bank - Name",
                "Beneficiary Bank - City",
                "Beneficiary Bank - Country",
                "Beneficiary - Account Number",
                "Beneficiary - Account Name",
                "Beneficiary - Address line 1",
                "Reserved for future use",
                "Reserved for future use",
                "Beneficiary - City",
                "Beneficiary - State",
                "Beneficiary - Postcode",
                "Beneficiary - Country",
                "Beneficiary Payment Details",
            },
            fields.Select(field => field.CbaResponseField));
    }

    [Fact]
    public void MapFields_previously_missing_reserved_and_unused_fields_appear_at_their_spec_position_with_empty_values()
    {
        var fields = ImtRecordMapper.MapFields(ValidInstruction(), DebitAccountNumber);

        var expected = new (int Index, string Name)[]
        {
            (7, "Dealer Code"),
            (8, "Dealer Exchange Rate"),
            (11, "Intermediary Bank - City"),
            (15, "Beneficiary Bank - City"),
            (20, "Reserved for future use"),
            (21, "Reserved for future use"),
            (22, "Beneficiary - City"),
            (23, "Beneficiary - State"),
            (24, "Beneficiary - Postcode"),
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
    public void MapFields_cba_response_values_match_the_same_values_written_into_the_csv_row()
    {
        var record = ImtRecordMapper.Map(ValidInstruction(), DebitAccountNumber);
        var recordFields = Fields(record);
        var fields = ImtRecordMapper.MapFields(ValidInstruction(), DebitAccountNumber);

        Assert.Equal(recordFields[0], fields.Single(f => f.CbaResponseField == "Transaction Type").CbaResponseValue);
        Assert.Equal(recordFields[6], fields.Single(f => f.CbaResponseField == "Debit Account - Account Number").CbaResponseValue);
        Assert.Equal(recordFields[12], fields.Single(f => f.CbaResponseField == "Intermediary Institution - Country").CbaResponseValue);
        Assert.Equal(recordFields[19], fields.Single(f => f.CbaResponseField == "Beneficiary - Address line 1").CbaResponseValue);
        Assert.Equal(recordFields[25], fields.Single(f => f.CbaResponseField == "Beneficiary - Country").CbaResponseValue);
    }
}
