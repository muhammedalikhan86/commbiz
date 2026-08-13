namespace CommBiz.Api.Features.DirectEntry;

// Trailer Record mapping (F-007, architecture.md §3/§4 step 5; docs/stash/Direct Entry - File Specification
// CommBiz.md §3): totals are computed from the batch's Detail Records so the file is self-balancing, per
// ADR-004 (manual mapping only, no AutoMapper). F-014 adds the self-balancing (contra) detail record,
// which always posts the batch total against the opposite side of whichever direction is configured — so
// credit total and debit total are now always equal, and net total is always zero.
public static class DirectEntryTrailerRecordMapper
{
    private const string RecordType = "7";
    private const string BsbNumber = "999-999";

    public static string Map(IReadOnlyList<PaymentInstructionRequest> instructions, DirectEntrySettings settings)
    {
        var amountTotalInCents = DirectEntryAmountTotals.SumAmountInCents(instructions);
        var amountTotalField = amountTotalInCents.ToString().PadLeft(10, '0');

        return
            RecordType +
            BsbNumber +
            new string(' ', 12) +
            "0000000000" + // File Net Total Amount - always zero, per the self-balancing record (F-014)
            amountTotalField + // File Credit Total Amount
            amountTotalField + // File Debit Total Amount
            new string(' ', 24) +
            (instructions.Count + 1).ToString().PadLeft(6, '0') + // +1 for the self-balancing record (F-014)
            new string(' ', 40);
    }
}
