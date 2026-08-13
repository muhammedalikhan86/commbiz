namespace CommBiz.Api.Features.DirectEntry;

// Trailer Record mapping (F-007, architecture.md §3/§4 step 5; docs/stash/Direct Entry - File Specification
// CommBiz.md §3): totals are computed from the batch's Detail Records so the file is self-balancing, per
// ADR-004 (manual mapping only, no AutoMapper).
public static class DirectEntryTrailerRecordMapper
{
    private const string RecordType = "7";
    private const string BsbNumber = "999-999";

    // Only debit code per the Direct Entry spec; all other codes reaching this feature are credits
    // (any other code is already rejected by F-005's validator).
    private const string DebitTransactionCode = "13";

    public static string Map(IReadOnlyList<PaymentInstructionRequest> instructions)
    {
        var creditTotal = instructions
            .Where(instruction => instruction.TransactionCode != DebitTransactionCode)
            .Sum(instruction => instruction.AmountInCents);
        var debitTotal = instructions
            .Where(instruction => instruction.TransactionCode == DebitTransactionCode)
            .Sum(instruction => instruction.AmountInCents);
        var netTotal = Math.Abs(creditTotal - debitTotal);

        return
            RecordType +
            BsbNumber +
            new string(' ', 12) +
            netTotal.ToString().PadLeft(10, '0') +
            creditTotal.ToString().PadLeft(10, '0') +
            debitTotal.ToString().PadLeft(10, '0') +
            new string(' ', 24) +
            instructions.Count.ToString().PadLeft(6, '0') +
            new string(' ', 40);
    }
}
