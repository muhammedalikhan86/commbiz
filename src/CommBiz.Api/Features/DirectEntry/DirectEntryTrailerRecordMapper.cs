using CommBiz.Api.Features.Shared;

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
    private const string NetTotalAmount = "0000000000"; // always zero, per the self-balancing record (F-014)

    public static string Map(IReadOnlyList<PaymentInstructionRequest> instructions, DirectEntrySettings settings)
    {
        var values = ResolveValues(instructions);

        return
            RecordType +
            BsbNumber +
            new string(' ', 12) +
            NetTotalAmount +
            values.AmountTotalField + // File Credit Total Amount
            values.AmountTotalField + // File Debit Total Amount
            new string(' ', 24) +
            values.RecordCountField +
            new string(' ', 40);
    }

    // F-021 correction: same resolved values as Map, so the field-mapping breakdown can never drift
    // from ConvertedText - one entry per Trailer field position (9 total), including the 3 Blank
    // filler positions whose cbaResponseValue is the literal spaces Map writes, not an empty string.
    public static IReadOnlyList<FieldMapping> MapFields(
        IReadOnlyList<PaymentInstructionRequest> instructions, DirectEntrySettings settings)
    {
        var values = ResolveValues(instructions);

        return
        [
            new(nameof(RecordType), RecordType, "Record Type", RecordType),
            new(nameof(BsbNumber), BsbNumber, "BSB Number", BsbNumber),
            new("", "", "Blank", new string(' ', 12)),
            new(nameof(NetTotalAmount), NetTotalAmount, "File (User) Net Total Amount", NetTotalAmount),
            new(
                "Amount",
                values.AmountTotalInCents.ToString(),
                "File (User) Credit Total Amount",
                values.AmountTotalField),
            new(
                "Amount",
                values.AmountTotalInCents.ToString(),
                "File (User) Debit Total Amount",
                values.AmountTotalField),
            new("", "", "Blank", new string(' ', 24)),
            new(
                "Instructions.Count",
                instructions.Count.ToString(),
                "File (User) Count of Record Type 1",
                values.RecordCountField),
            new("", "", "Blank", new string(' ', 40)),
        ];
    }

    private static (long AmountTotalInCents, string AmountTotalField, string RecordCountField) ResolveValues(
        IReadOnlyList<PaymentInstructionRequest> instructions)
    {
        var amountTotalInCents = DirectEntryAmountTotals.SumAmountInCents(instructions);
        var amountTotalField = amountTotalInCents.ToString().PadLeft(10, '0');
        // +1 for the self-balancing record (F-014)
        var recordCountField = (instructions.Count + 1).ToString().PadLeft(6, '0');

        return (amountTotalInCents, amountTotalField, recordCountField);
    }
}
