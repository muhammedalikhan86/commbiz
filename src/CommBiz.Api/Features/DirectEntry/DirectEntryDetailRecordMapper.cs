using CommBiz.Api.Features.Shared;
using static CommBiz.Api.Features.Shared.MappingUtilities;

namespace CommBiz.Api.Features.DirectEntry;

// Detail Record mapping (F-006, architecture.md §3/§4 step 5; docs/stash/Direct Entry - File Specification
// CommBiz.md §2): manual field concatenation only, per ADR-004 (no AutoMapper). Runs only on instructions
// that already passed the top-level Payment Type Router and the F-005 validator.
public static class DirectEntryDetailRecordMapper
{
    private const string RecordType = "1";

    // Spec: "Care should be exercised to ensure inclusion of 'N' symbol" — combined with withholding tax
    // always being zero (static config), "N" (ordinary, non-withholding) is correct for every instruction.
    private const string Indicator = "N";

    public static string Map(PaymentInstructionRequest instruction, DirectEntrySettings settings) =>
        RecordType +
        settings.TraceAccountBsb +
        settings.TraceAccountAccNo.PadLeft(9) +
        Indicator +
        settings.TransactionCode +
        AmountToCents(instruction.Amount).ToString().PadLeft(10, '0') +
        FixedWidth(settings.NameOfRemitter, 32) +
        FixedWidth(settings.LodgementReferenceDetails, 18) +
        FormatBsb(instruction.DestinationBankBsb) +
        instruction.DestinationBankAccountNo.PadLeft(9) +
        FixedWidth(instruction.DestinationBankAccountName, 16) +
        settings.AmountOfWithholdingTax;

    // F-021: same field values already written into Map's output - never recomputed separately.
    public static IReadOnlyList<FieldMapping> MapFields(PaymentInstructionRequest instruction, DirectEntrySettings settings) =>
    [
        new(nameof(RecordType), RecordType, "Record Type", RecordType),
        new(nameof(DirectEntrySettings.TraceAccountBsb), settings.TraceAccountBsb, "BSB Number", settings.TraceAccountBsb),
        new(
            nameof(DirectEntrySettings.TraceAccountAccNo),
            settings.TraceAccountAccNo,
            "Account Number to be Credited/Debited",
            settings.TraceAccountAccNo.PadLeft(9)),
        new(nameof(Indicator), Indicator, "Indicator", Indicator),
        new(nameof(DirectEntrySettings.TransactionCode), settings.TransactionCode, "Transaction Code", settings.TransactionCode),
        new(
            nameof(instruction.Amount),
            instruction.Amount.ToString(),
            "Amount",
            AmountToCents(instruction.Amount).ToString().PadLeft(10, '0')),
        new(
            nameof(DirectEntrySettings.NameOfRemitter),
            settings.NameOfRemitter,
            "Title of Account to be Credited/Debited",
            FixedWidth(settings.NameOfRemitter, 32)),
        new(
            nameof(DirectEntrySettings.LodgementReferenceDetails),
            settings.LodgementReferenceDetails,
            "Lodgement Reference",
            FixedWidth(settings.LodgementReferenceDetails, 18)),
        new(
            nameof(instruction.DestinationBankBsb),
            instruction.DestinationBankBsb,
            "Trace BSB Number",
            FormatBsb(instruction.DestinationBankBsb)),
        new(
            nameof(instruction.DestinationBankAccountNo),
            instruction.DestinationBankAccountNo,
            "Trace Account Number",
            instruction.DestinationBankAccountNo.PadLeft(9)),
        new(
            nameof(instruction.DestinationBankAccountName),
            instruction.DestinationBankAccountName,
            "Name of Remitter",
            FixedWidth(instruction.DestinationBankAccountName, 16)),
        new(
            nameof(DirectEntrySettings.AmountOfWithholdingTax),
            settings.AmountOfWithholdingTax,
            "Amount of withholding tax",
            settings.AmountOfWithholdingTax),
    ];

    // Reformats a raw 6-digit BSB (e.g. "015141") into the spec's nnn-nnn shape (e.g. "015-141").
    private static string FormatBsb(string bsb) => bsb[..3] + "-" + bsb[3..];
}
