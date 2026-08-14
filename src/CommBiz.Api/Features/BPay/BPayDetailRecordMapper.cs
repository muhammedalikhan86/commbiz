namespace CommBiz.Api.Features.BPay;

// Payment Details Record mapping (F-016, docs/stash/BPay Payments - CommBiz File Specification.md
// §1.3): manual field concatenation only, per ADR-004 (no AutoMapper). 25 comma-separated fields;
// every field not explicitly listed by the spec for this record type stays empty.
public static class BPayDetailRecordMapper
{
    private const string RecordType = "50";

    public static string Map(BPayPaymentInstructionRequest instruction) =>
        string.Join(
            ",",
            RecordType,
            "", "", "", "", "", "", "", // fields 2-8: File Creation Date/Time/Number, Payment Account, Payment Date, Number of Payment Records, Currency Code - empty on detail records
            instruction.BPayBillerCode, // field 9: Biller Code
            "", // field 10: Service Code - empty
            instruction.BPayReference, // field 11: Customer Reference Number
            "", "", // fields 12-13: Payment Method, Entry Method - empty
            AmountToCents(instruction.Amount).ToString(), // field 14: Amount, in cents
            "", "", "", "", "", "", "", "", "", "", ""); // fields 15-25: empty

    private static long AmountToCents(decimal amount) =>
        (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);
}
