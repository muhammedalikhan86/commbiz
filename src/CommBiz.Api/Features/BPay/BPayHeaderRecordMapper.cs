namespace CommBiz.Api.Features.BPay;

// Header Record mapping (F-016, docs/stash/BPay Payments - CommBiz File Specification.md §1.3): manual
// field concatenation only, per ADR-004 (no AutoMapper). CSV, comma-delimited, no trailing comma -
// fields are NOT fixed-width/padded, unlike Direct Entry.
public static class BPayHeaderRecordMapper
{
    private const string RecordType = "01";

    // A BPay file has exactly one header date but each instruction carries its own PaymentDate; the
    // earliest instruction's date in the batch is used as the header's Payment Date, consistent with
    // how Direct Entry's header already picks the earliest instruction date.
    public static string Map(IReadOnlyList<BPayPaymentInstructionRequest> instructions, BPaySettings settings)
    {
        var now = DateTime.UtcNow;
        var paymentDate = instructions.Min(instruction => instruction.PaymentDate.Date);
        var totalAmountInCents = instructions.Sum(instruction => AmountToCents(instruction.Amount));

        return string.Join(
            ",",
            RecordType,
            now.ToString("yyyyMMdd"),
            now.ToString("HHmmss"),
            settings.FileNumber,
            settings.FundingAccount,
            paymentDate.ToString("yyyyMMdd"),
            instructions.Count.ToString(),
            totalAmountInCents.ToString());
    }

    private static long AmountToCents(decimal amount) =>
        (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);
}
