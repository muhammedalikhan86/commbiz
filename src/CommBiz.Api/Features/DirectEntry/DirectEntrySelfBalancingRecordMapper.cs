using CommBiz.Api.Features.Shared;
using static CommBiz.Api.Features.Shared.MappingUtilities;

namespace CommBiz.Api.Features.DirectEntry;

// Self-balancing (contra) Detail Record mapping (F-014, architecture.md §3/§4 step 5; docs/stash/Direct
// Entry - File Specification CommBiz.md §2): CommBank requires every submitted file to include a contra
// entry against the user's settlement account that offsets the batch total, so the file's credit/debit
// totals reconcile (see DirectEntryTrailerRecordMapper). Manual field concatenation only, per ADR-004.
// This record posts the batch total against Shaw's own settlement account - every field except the
// amount is a fixed literal, never sourced from settings or any instruction (matches the legacy DE
// implementation's GetSelfBalancingRecord).
public static class DirectEntrySelfBalancingRecordMapper
{
    private const string RecordType = "1";
    private const string Indicator = " ";
    private const string DebitTransactionCode = "13";
    private const string CreditTransactionCode = "50";
    private const string Title = "SHAW AND PARTNERS LIMITED";

    public static string Map(IReadOnlyList<PaymentInstructionRequest> instructions, DirectEntrySettings settings)
    {
        var transactionCode = ResolveTransactionCode(settings);
        var totalAmountInCents = DirectEntryAmountTotals.SumAmountInCents(instructions);

        return
            RecordType +
            settings.TraceAccountBsb +
            settings.SelfBalancingAccountNo.PadLeft(9) +
            Indicator +
            transactionCode +
            totalAmountInCents.ToString().PadLeft(10, '0') +
            FixedWidth(Title, 32) +
            FixedWidth(settings.SelfBalancingLodgementReferenceDetails, 18) +
            settings.TraceAccountBsb +
            settings.SelfBalancingAccountNo.PadLeft(9) +
            FixedWidth(settings.SelfBalancingNameOfRemitter, 16) +
            settings.AmountOfWithholdingTax;
    }

    // F-021: same resolved values as Map, so the field-mapping breakdown can never drift from ConvertedText.
    public static IReadOnlyList<FieldMapping> MapFields(
        IReadOnlyList<PaymentInstructionRequest> instructions, DirectEntrySettings settings)
    {
        var transactionCode = ResolveTransactionCode(settings);
        var totalAmountInCents = DirectEntryAmountTotals.SumAmountInCents(instructions);

        return
        [
            new(nameof(RecordType), RecordType, "Record Type", RecordType),
            new(nameof(DirectEntrySettings.TraceAccountBsb), settings.TraceAccountBsb, "BSB Number", settings.TraceAccountBsb),
            new(
                nameof(DirectEntrySettings.SelfBalancingAccountNo),
                settings.SelfBalancingAccountNo,
                "Account Number to be Credited/Debited",
                settings.SelfBalancingAccountNo.PadLeft(9)),
            new(nameof(Indicator), Indicator, "Indicator", Indicator),
            new(
                nameof(DirectEntrySettings.TransactionCode),
                settings.TransactionCode,
                "Transaction Code",
                transactionCode),
            new("Amount", totalAmountInCents.ToString(), "Amount", totalAmountInCents.ToString().PadLeft(10, '0')),
            new(nameof(Title), Title, "Title of Account to be Credited/Debited", FixedWidth(Title, 32)),
            new(
                nameof(DirectEntrySettings.SelfBalancingLodgementReferenceDetails),
                settings.SelfBalancingLodgementReferenceDetails,
                "Lodgement Reference",
                FixedWidth(settings.SelfBalancingLodgementReferenceDetails, 18)),
            new(nameof(DirectEntrySettings.TraceAccountBsb), settings.TraceAccountBsb, "Trace BSB Number", settings.TraceAccountBsb),
            new(
                nameof(DirectEntrySettings.SelfBalancingAccountNo),
                settings.SelfBalancingAccountNo,
                "Trace Account Number",
                settings.SelfBalancingAccountNo.PadLeft(9)),
            new(
                nameof(DirectEntrySettings.SelfBalancingNameOfRemitter),
                settings.SelfBalancingNameOfRemitter,
                "Name of Remitter",
                FixedWidth(settings.SelfBalancingNameOfRemitter, 16)),
            new(
                nameof(DirectEntrySettings.AmountOfWithholdingTax),
                settings.AmountOfWithholdingTax,
                "Amount of withholding tax",
                settings.AmountOfWithholdingTax),
        ];
    }

    // Must be the inverse of the batch's configured direction - CBA cross-checks the Trailer's Credit/
    // Debit totals against the actual sum of credit-coded vs debit-coded Detail records in the file, so
    // a contra coded the same direction as the real payments leaves one side of that total at zero.
    private static string ResolveTransactionCode(DirectEntrySettings settings) =>
        settings.TransactionCode == DebitTransactionCode ? CreditTransactionCode : DebitTransactionCode;
}
