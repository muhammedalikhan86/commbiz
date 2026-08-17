using CommBiz.Api.Features.PriorityPayments;

namespace CommBiz.Api.Tests.PriorityPayments;

public class PriorityPaymentRecordMapperTests
{
    private static readonly PriorityPaymentsSettings Settings = new()
    {
        DebitAccountBsb = "062-000",
        DebitAccountNumber = "2112 0075",
        DebitAccountName = "SHAW - AUD TRUST ACCOUNT",
    };

    // Confirmed real sample payload from the Task Packet.
    private static PriorityPaymentInstructionRequest ValidInstruction() =>
        new(
            PaymentTypeCode: "RTGS",
            PaymentSourceTypeCode: "CMA",
            SourceBankAccountName: "J & D SARGENT SUPER CO PTY LTD ATF JASON & DYNA SARGENT SF",
            SourceBankAccountNo: "114316871",
            SourceBankBsb: "012141",
            DestinationBankAccountName: "ORS APP GATB",
            DestinationBankAccountNo: "838629371",
            DestinationBankBsb: "012110",
            PaymentDate: new DateTime(2026, 4, 10, 10, 0, 0),
            SourceCurrency: "AUD",
            SourceAmount: 0.0m,
            Amount: 10775.0m,
            Notes: "Accounts has been paid to before.",
            BeneficiaryAddress: null);

    private static string[] Fields(string record) => record.Split(',');

    private static string DebitAccountNumber => PriorityPaymentRecordMapper.DeriveDebitAccountNumber(Settings);

    [Fact]
    public void Record_has_exactly_27_comma_separated_fields()
    {
        var record = PriorityPaymentRecordMapper.Map(ValidInstruction(), DebitAccountNumber);

        Assert.Equal(27, Fields(record).Length);
    }

    [Fact]
    public void Field_1_is_the_literal_constant_PP_never_RTGS()
    {
        var record = PriorityPaymentRecordMapper.Map(ValidInstruction(), DebitAccountNumber);

        Assert.Equal("PP", Fields(record)[0]);
    }

    [Fact]
    public void Field_2_is_Notes()
    {
        var record = PriorityPaymentRecordMapper.Map(ValidInstruction(), DebitAccountNumber);

        Assert.Equal("Accounts has been paid to before.", Fields(record)[1]);
    }

    [Fact]
    public void Field_3_process_date_is_formatted_YYMMDD()
    {
        var record = PriorityPaymentRecordMapper.Map(ValidInstruction(), DebitAccountNumber);

        Assert.Equal("260410", Fields(record)[2]);
    }

    [Fact]
    public void Field_5_payment_amount_matches_the_sample_value()
    {
        var record = PriorityPaymentRecordMapper.Map(ValidInstruction(), DebitAccountNumber);

        Assert.Equal("10775.0", Fields(record)[4]);
    }

    [Fact]
    public void Field_7_debit_account_number_is_derived_from_static_settings()
    {
        var record = PriorityPaymentRecordMapper.Map(ValidInstruction(), DebitAccountNumber);

        // BSB "062-000" -> last 4 digits "2000"; account number "2112 0075" -> spaces stripped "21120075"
        Assert.Equal("200021120075", Fields(record)[6]);
    }

    [Fact]
    public void Field_14_beneficiary_bank_bsb_is_written_as_plain_6_digits_not_hyphenated()
    {
        var record = PriorityPaymentRecordMapper.Map(ValidInstruction(), DebitAccountNumber);

        Assert.Equal("012110", Fields(record)[13]);
    }

    [Fact]
    public void Field_18_is_beneficiary_account_number()
    {
        var record = PriorityPaymentRecordMapper.Map(ValidInstruction(), DebitAccountNumber);

        Assert.Equal("838629371", Fields(record)[17]);
    }

    [Fact]
    public void Field_19_is_beneficiary_account_name()
    {
        var record = PriorityPaymentRecordMapper.Map(ValidInstruction(), DebitAccountNumber);

        Assert.Equal("ORS APP GATB", Fields(record)[18]);
    }

    [Fact]
    public void Field_20_null_beneficiary_address_is_written_as_empty_string()
    {
        var record = PriorityPaymentRecordMapper.Map(ValidInstruction(), DebitAccountNumber);

        Assert.Equal(string.Empty, Fields(record)[19]);
    }

    [Fact]
    public void Field_20_beneficiary_address_hyphens_are_sanitized_to_spaces()
    {
        var instruction = ValidInstruction() with { BeneficiaryAddress = "9101 Alta-Drive" };
        var record = PriorityPaymentRecordMapper.Map(instruction, DebitAccountNumber);

        Assert.Equal("9101 Alta Drive", Fields(record)[19]);
    }

    [Fact]
    public void Fields_4_6_8_9_10_11_12_13_15_16_17_21_through_27_are_always_empty()
    {
        var record = PriorityPaymentRecordMapper.Map(ValidInstruction(), DebitAccountNumber);
        var fields = Fields(record);
        var alwaysEmptyIndexes = new[] { 3, 5, 7, 8, 9, 10, 11, 12, 14, 15, 16, 20, 21, 22, 23, 24, 25, 26 };

        foreach (var index in alwaysEmptyIndexes)
        {
            Assert.Equal(string.Empty, fields[index]);
        }
    }

    [Fact]
    public void MapFields_returns_all_27_fields_in_spec_order()
    {
        var fields = PriorityPaymentRecordMapper.MapFields(ValidInstruction(), DebitAccountNumber);

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
                "Beneficiary - Address line 2",
                "Beneficiary - Address line 3",
                "Beneficiary - City",
                "Beneficiary - State",
                "Beneficiary - Postcode",
                "Beneficiary - Country Code",
                "Beneficiary Payment Details",
            },
            fields.Select(field => field.CbaResponseField));
    }

    [Fact]
    public void MapFields_returns_exactly_27_entries()
    {
        var fields = PriorityPaymentRecordMapper.MapFields(ValidInstruction(), DebitAccountNumber);

        Assert.Equal(27, fields.Count);
    }

    [Fact]
    public void MapFields_blank_positions_have_empty_request_and_response_values()
    {
        var fields = PriorityPaymentRecordMapper.MapFields(ValidInstruction(), DebitAccountNumber);

        var blankFieldNames = new[]
        {
            "Payment Currency",
            "Debit Amount",
            "Dealer Code",
            "Dealer Exchange Rate",
            "Intermediary Bank - Bank Code",
            "Intermediary Bank - Name",
            "Intermediary Bank - City",
            "Intermediary Institution - Country",
            "Beneficiary Bank - Name",
            "Beneficiary Bank - City",
            "Beneficiary Bank - Country",
            "Beneficiary - Address line 2",
            "Beneficiary - Address line 3",
            "Beneficiary - City",
            "Beneficiary - State",
            "Beneficiary - Postcode",
            "Beneficiary - Country Code",
            "Beneficiary Payment Details",
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
    public void MapFields_field_7_uses_the_combined_debit_account_descriptive_identifier()
    {
        var fields = PriorityPaymentRecordMapper.MapFields(ValidInstruction(), DebitAccountNumber);

        var field = fields.Single(f => f.CbaResponseField == "Debit Account - Account Number");
        Assert.Equal("DebitAccountBsb+DebitAccountNumber", field.RequestField);
        Assert.Equal(DebitAccountNumber, field.CbaResponseValue);
    }

    [Fact]
    public void MapFields_cba_response_values_match_the_same_values_written_into_the_csv_row()
    {
        var record = PriorityPaymentRecordMapper.Map(ValidInstruction(), DebitAccountNumber);
        var recordFields = Fields(record);
        var fields = PriorityPaymentRecordMapper.MapFields(ValidInstruction(), DebitAccountNumber);

        Assert.Equal(recordFields[0], fields.Single(f => f.CbaResponseField == "Transaction Type").CbaResponseValue);
        Assert.Equal(recordFields[6], fields.Single(f => f.CbaResponseField == "Debit Account - Account Number").CbaResponseValue);
        Assert.Equal(recordFields[13], fields.Single(f => f.CbaResponseField == "Beneficiary Bank - Bank Code").CbaResponseValue);
        Assert.Equal(recordFields[17], fields.Single(f => f.CbaResponseField == "Beneficiary - Account Number").CbaResponseValue);
        Assert.Equal(recordFields[18], fields.Single(f => f.CbaResponseField == "Beneficiary - Account Name").CbaResponseValue);
        Assert.Equal(recordFields[19], fields.Single(f => f.CbaResponseField == "Beneficiary - Address line 1").CbaResponseValue);
    }
}
