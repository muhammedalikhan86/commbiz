namespace CommBiz.Api.Features.DirectEntry;

// Header Record mapping (F-007, architecture.md §3/§4 step 5; docs/stash/Direct Entry - File Specification
// CommBiz.md §1): manual field concatenation only, per ADR-004 (no AutoMapper).
public static class DirectEntryHeaderRecordMapper
{
    private const string RecordType = "0";
    private const string ReelSequenceNumber = "01";

    // A Direct Entry file has exactly one header date but each instruction carries its own PaymentDate;
    // the earliest instruction's date in the batch is used as the interim header date (see Assumptions).
    public static string Map(IReadOnlyList<PaymentInstructionRequest> instructions, DirectEntrySettings settings)
    {
        var dateToBeProcessed = instructions.Min(instruction => instruction.PaymentDate.Date);

        return
            RecordType +
            new string(' ', 17) +
            ReelSequenceNumber +
            settings.InstitutionCode +
            new string(' ', 7) +
            FixedWidth(settings.NameOfUserSupplyingFile, 26) +
            settings.UserIdentificationNumber.PadLeft(6, '0') +
            FixedWidth(settings.DescriptionOfEntriesOnFile, 12) +
            dateToBeProcessed.ToString("ddMMyy") +
            new string(' ', 40);
    }

    // Truncates rather than overflows the fixed-width record if a config value is longer than its field.
    private static string FixedWidth(string value, int width) =>
        value.Length >= width ? value[..width] : value.PadRight(width);
}
