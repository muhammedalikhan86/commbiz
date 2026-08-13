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
        // Inverse of the batch's configured direction, so this record offsets the real detail records.
        var transactionCode = settings.TransactionCode == DebitTransactionCode
            ? CreditTransactionCode
            : DebitTransactionCode;

        return
            RecordType +
            settings.TraceAccountBsb +
            settings.TraceAccountAccNo.PadLeft(9) +
            Indicator +
            transactionCode +
            DirectEntryAmountTotals.SumAmountInCents(instructions).ToString().PadLeft(10, '0') +
            FixedWidth(settings.Title, 32) +
            FixedWidth(settings.LodgementReferenceDetails, 18) +
            settings.TraceAccountBsb +
            settings.TraceAccountAccNo.PadLeft(9) +
            FixedWidth(settings.NameOfRemitter, 16) +
            settings.AmountOfWithholdingTax;
    }

    // Truncates rather than overflows the fixed-width record if a config value is longer than its field.
    private static string FixedWidth(string value, int width) =>
        value.Length >= width ? value[..width] : value.PadRight(width);
}
