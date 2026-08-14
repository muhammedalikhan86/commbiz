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
        FormatBsb(instruction.SourceBankBsb) +
        instruction.SourceBankAccountNo.PadLeft(9) +
        Indicator +
        settings.TransactionCode +
        AmountToCents(instruction.Amount).ToString().PadLeft(10, '0') +
        FixedWidth(settings.Title, 32) +
        FixedWidth(settings.LodgementReferenceDetails, 18) +
        settings.TraceAccountBsb +
        settings.TraceAccountAccNo.PadLeft(9) +
        FixedWidth(settings.NameOfRemitter, 16) +
        settings.AmountOfWithholdingTax;

    // Reformats a raw 6-digit BSB (e.g. "015141") into the spec's nnn-nnn shape (e.g. "015-141").
    private static string FormatBsb(string bsb) => bsb[..3] + "-" + bsb[3..];

    private static long AmountToCents(decimal amount) =>
        (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);

    // Truncates rather than overflows the fixed-width record if a config value is longer than its field.
    private static string FixedWidth(string value, int width) =>
        value.Length >= width ? value[..width] : value.PadRight(width);
}
