namespace CommBiz.Api.Features.DirectEntry;

// Detail Record mapping (F-006, architecture.md §3/§4 step 5; docs/stash/Direct Entry - File Specification
// CommBiz.md §2): manual field concatenation only, per ADR-004 (no AutoMapper). Runs only on instructions
// that already passed the F-004 router and F-005 validator.
public static class DirectEntryDetailRecordMapper
{
    private const string RecordType = "1";

    public static string Map(PaymentInstructionRequest instruction) =>
        RecordType +
        instruction.Bsb +
        instruction.AccountNumber.PadLeft(9) +
        instruction.Indicator.PadRight(1) +
        instruction.TransactionCode +
        instruction.AmountInCents.ToString().PadLeft(10, '0') +
        instruction.AccountTitle.PadRight(32) +
        instruction.LodgementReference.PadRight(18) +
        instruction.TraceBsb +
        instruction.TraceAccountNumber.PadLeft(9) +
        instruction.RemitterName.PadRight(16) +
        instruction.WithholdingTaxAmountInCents.ToString().PadLeft(8, '0');
}
