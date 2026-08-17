using CommBiz.Api.Features.Shared;
using static CommBiz.Api.Features.Shared.MappingUtilities;

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
    public static string Map(IReadOnlyList<BPayPaymentInstructionRequest> instructions, BPaySettings settings) =>
        Map(instructions, settings, DateTime.UtcNow);

    // F-021: overload taking an explicit `now` so a caller building both ConvertedText and Mappings for
    // the same header can guarantee they share one timestamp - see ConvertBPayBatchCommand.
    public static string Map(IReadOnlyList<BPayPaymentInstructionRequest> instructions, BPaySettings settings, DateTime now)
    {
        var values = ResolveValues(instructions, settings, now);

        return string.Join(
            ",",
            RecordType,
            values.Now.ToString("yyyyMMdd"),
            values.Now.ToString("HHmmss"),
            settings.FileNumber,
            settings.FundingAccount,
            values.PaymentDate.ToString("yyyyMMdd"),
            instructions.Count.ToString(),
            values.TotalAmountInCents.ToString());
    }

    public static IReadOnlyList<FieldMapping> MapFields(
        IReadOnlyList<BPayPaymentInstructionRequest> instructions, BPaySettings settings) =>
        MapFields(instructions, settings, DateTime.UtcNow);

    // F-021: same resolved values as Map, so the field-mapping breakdown can never drift from ConvertedText.
    public static IReadOnlyList<FieldMapping> MapFields(
        IReadOnlyList<BPayPaymentInstructionRequest> instructions, BPaySettings settings, DateTime now)
    {
        var values = ResolveValues(instructions, settings, now);

        return
        [
            new(nameof(RecordType), RecordType, "Record Type", RecordType),
            new(
                "DateTime.UtcNow",
                values.Now.ToString("O"),
                "File Creation Date",
                values.Now.ToString("yyyyMMdd")),
            new(
                "DateTime.UtcNow",
                values.Now.ToString("O"),
                "File Creation Time",
                values.Now.ToString("HHmmss")),
            new(nameof(BPaySettings.FileNumber), settings.FileNumber, "File Number", settings.FileNumber),
            new(nameof(BPaySettings.FundingAccount), settings.FundingAccount, "Payment Account", settings.FundingAccount),
            new(
                nameof(BPayPaymentInstructionRequest.PaymentDate),
                values.PaymentDate.ToString("O"),
                "Payment Date",
                values.PaymentDate.ToString("yyyyMMdd")),
            new(
                "Instructions.Count",
                instructions.Count.ToString(),
                "Number of Payment Records",
                instructions.Count.ToString()),
            new(
                nameof(BPayPaymentInstructionRequest.Amount),
                values.TotalAmountInCents.ToString(),
                "Total Amount of Payments",
                values.TotalAmountInCents.ToString()),
        ];
    }

    private static (DateTime Now, DateTime PaymentDate, long TotalAmountInCents) ResolveValues(
        IReadOnlyList<BPayPaymentInstructionRequest> instructions, BPaySettings settings, DateTime now) =>
        (
            now,
            instructions.Min(instruction => instruction.PaymentDate.Date),
            instructions.Sum(instruction => AmountToCents(instruction.Amount)));
}
