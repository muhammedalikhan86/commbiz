using CommBiz.Api.Features.Shared;
using static CommBiz.Api.Features.Shared.MappingUtilities;

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

    // F-021 correction: one entry per Payment Details field position (25 total, same order as Map),
    // including reserved/unused fields 2-8/10/12-13/15-25 - a dropped position would break
    // correspondence between Fields and the raw comma-separated output line.
    public static IReadOnlyList<FieldMapping> MapFields(BPayPaymentInstructionRequest instruction) =>
    [
        new(nameof(RecordType), RecordType, "Record Type", RecordType),
        new("", "", "File Creation Date", ""),
        new("", "", "File Creation Time", ""),
        new("", "", "File Number", ""),
        new("", "", "Payment Account", ""),
        new("", "", "Payment Date", ""),
        new("", "", "Number of Payment Records", ""),
        new("", "", "Currency Code of Payment", ""),
        new(nameof(instruction.BPayBillerCode), instruction.BPayBillerCode, "Biller Code", instruction.BPayBillerCode),
        new("", "", "Service Code", ""),
        new(nameof(instruction.BPayReference), instruction.BPayReference, "Customer Reference Number", instruction.BPayReference),
        new("", "", "Payment Method", ""),
        new("", "", "Entry Method", ""),
        new(
            nameof(instruction.Amount),
            instruction.Amount.ToString(),
            "Amount",
            AmountToCents(instruction.Amount).ToString()),
        new("", "", "Transaction Reference Number", ""),
        new("", "", "Original Reference Number", ""),
        new("", "", "BPAY Settlement Date", ""),
        new("", "", "Date Payment Accepted", ""),
        new("", "", "Time Payment Accepted", ""),
        new("", "", "Payer Name", ""),
        new("", "", "Additional Reference Code", ""),
        new("", "", "Error Correction Reason", ""),
        new("", "", "Discount Method", ""),
        new("", "", "Discount Reference", ""),
        new("", "", "Discretionary Data", ""),
    ];
}
