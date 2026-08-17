using CommBiz.Api.Features.Shared;
using static CommBiz.Api.Features.Shared.MappingUtilities;

namespace CommBiz.Api.Features.DirectEntry;

// Self-balancing (contra) Detail Record mapping (F-014, architecture.md §3/§4 step 5; docs/stash/Direct
// Entry - File Specification CommBiz.md §2): CommBank requires every submitted file to include a contra
// entry against the user's settlement account that offsets the batch total, so the file's credit/debit
// totals reconcile (see DirectEntryTrailerRecordMapper). Manual field concatenation only, per ADR-004.
public static class DirectEntrySelfBalancingRecordMapper
{
    private const string RecordType = "1";
    private const string DebitTransactionCode = "13";
    private const string CreditTransactionCode = "50";
    private const string Indicator = "N";

    public static string Map(IReadOnlyList<PaymentInstructionRequest> instructions, DirectEntrySettings settings)
    {
        var values = ResolveValues(instructions, settings);

        return
            RecordType +
            settings.TraceAccountBsb +
            settings.TraceAccountAccNo.PadLeft(9) +
            Indicator +
            values.TransactionCode +
            values.TotalAmountInCents.ToString().PadLeft(10, '0') +
            FixedWidth(settings.Title, 32) +
            FixedWidth(settings.LodgementReferenceDetails, 18) +
            settings.TraceAccountBsb +
            settings.TraceAccountAccNo.PadLeft(9) +
            FixedWidth(settings.NameOfRemitter, 16) +
            settings.AmountOfWithholdingTax;
    }

    // F-021: same resolved values as Map, so the field-mapping breakdown can never drift from ConvertedText.
    public static IReadOnlyList<FieldMapping> MapFields(
        IReadOnlyList<PaymentInstructionRequest> instructions, DirectEntrySettings settings)
    {
        var values = ResolveValues(instructions, settings);

        return
        [
            new(nameof(RecordType), RecordType, "Record Type", RecordType),
            new(nameof(DirectEntrySettings.TraceAccountBsb), settings.TraceAccountBsb, "BSB Number", settings.TraceAccountBsb),
            new(
                nameof(DirectEntrySettings.TraceAccountAccNo),
                settings.TraceAccountAccNo,
                "Account Number to be Credited/Debited",
                settings.TraceAccountAccNo.PadLeft(9)),
            new(nameof(Indicator), Indicator, "Indicator", Indicator),
            new(
                nameof(DirectEntrySettings.TransactionCode),
                settings.TransactionCode,
                "Transaction Code",
                values.TransactionCode),
            new("Amount", values.TotalAmountInCents.ToString(), "Amount", values.TotalAmountInCents.ToString().PadLeft(10, '0')),
            new(nameof(DirectEntrySettings.Title), settings.Title, "Title of Account to be Credited/Debited", FixedWidth(settings.Title, 32)),
            new(
                nameof(DirectEntrySettings.LodgementReferenceDetails),
                settings.LodgementReferenceDetails,
                "Lodgement Reference",
                FixedWidth(settings.LodgementReferenceDetails, 18)),
            new(nameof(DirectEntrySettings.TraceAccountBsb), settings.TraceAccountBsb, "Trace BSB Number", settings.TraceAccountBsb),
            new(
                nameof(DirectEntrySettings.TraceAccountAccNo),
                settings.TraceAccountAccNo,
                "Trace Account Number",
                settings.TraceAccountAccNo.PadLeft(9)),
            new(
                nameof(DirectEntrySettings.NameOfRemitter),
                settings.NameOfRemitter,
                "Name of Remitter",
                FixedWidth(settings.NameOfRemitter, 16)),
            new(
                nameof(DirectEntrySettings.AmountOfWithholdingTax),
                settings.AmountOfWithholdingTax,
                "Amount of withholding tax",
                settings.AmountOfWithholdingTax),
        ];
    }

    private static (string TransactionCode, long TotalAmountInCents) ResolveValues(
        IReadOnlyList<PaymentInstructionRequest> instructions, DirectEntrySettings settings)
    {
        // Inverse of the batch's configured direction, so this record offsets the real detail records.
        var transactionCode = settings.TransactionCode == DebitTransactionCode
            ? CreditTransactionCode
            : DebitTransactionCode;

        return (transactionCode, DirectEntryAmountTotals.SumAmountInCents(instructions));
    }
}
