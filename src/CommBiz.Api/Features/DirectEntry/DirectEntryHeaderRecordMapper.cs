using CommBiz.Api.Features.Shared;
using static CommBiz.Api.Features.Shared.MappingUtilities;

namespace CommBiz.Api.Features.DirectEntry;

// Header Record mapping (F-007, architecture.md §3/§4 step 5; docs/stash/Direct Entry - File Specification
// CommBiz.md §1): manual field concatenation only, per ADR-004 (no AutoMapper).
public static class DirectEntryHeaderRecordMapper
{
    private const string RecordType = "0";
    private const string ReelSequenceNumber = "01";
    private const string InstitutionCode = "CBA";
    private const string UserIdentificationNumber = "301500";
    private const string NameOfUserSupplyingFile = "SHAW AND PARTNERS LIMITED";

    // A Direct Entry file has exactly one header date but each instruction carries its own PaymentDate;
    // the earliest instruction's date in the batch is used as the interim header date (see Assumptions).
    public static string Map(IReadOnlyList<PaymentInstructionRequest> instructions, DirectEntrySettings settings)
    {
        var values = ResolveValues(instructions, settings);

        return
            RecordType +
            new string(' ', 17) +
            ReelSequenceNumber +
            InstitutionCode +
            new string(' ', 7) +
            FixedWidth(NameOfUserSupplyingFile, 26) +
            UserIdentificationNumber.PadLeft(6, '0') +
            FixedWidth(values.DescriptionOfEntriesOnFile, 12) +
            values.DateToBeProcessed.ToString("ddMMyy") +
            new string(' ', 40);
    }

    // F-021 correction: same resolved values as Map, so the field-mapping breakdown can never drift
    // from ConvertedText - one entry per Header field position (10 total), including the 3 Blank
    // filler positions whose cbaResponseValue is the literal spaces Map writes, not an empty string.
    public static IReadOnlyList<FieldMapping> MapFields(
        IReadOnlyList<PaymentInstructionRequest> instructions, DirectEntrySettings settings)
    {
        var values = ResolveValues(instructions, settings);

        return
        [
            new(nameof(RecordType), RecordType, "Record Type", RecordType),
            new("", "", "Blank", new string(' ', 17)),
            new(nameof(ReelSequenceNumber), ReelSequenceNumber, "Reel Sequence Number", ReelSequenceNumber),
            new(
                nameof(InstitutionCode),
                InstitutionCode,
                "Name Of User Financial Institution",
                InstitutionCode),
            new("", "", "Blank", new string(' ', 7)),
            new(
                nameof(NameOfUserSupplyingFile),
                NameOfUserSupplyingFile,
                "Name of User Supplying File",
                FixedWidth(NameOfUserSupplyingFile, 26)),
            new(
                nameof(UserIdentificationNumber),
                UserIdentificationNumber,
                "Number of User Supplying File",
                UserIdentificationNumber.PadLeft(6, '0')),
            new(
                nameof(DirectEntrySettings.DescriptionOfEntriesOnFile),
                values.DescriptionOfEntriesOnFile,
                "Description of Entries on File",
                FixedWidth(values.DescriptionOfEntriesOnFile, 12)),
            new(
                nameof(PaymentInstructionRequest.PaymentDate),
                values.DateToBeProcessed.ToString("O"),
                "Date to be Processed",
                values.DateToBeProcessed.ToString("ddMMyy")),
            new("", "", "Blank", new string(' ', 40)),
        ];
    }

    private static (string DescriptionOfEntriesOnFile, DateTime DateToBeProcessed) ResolveValues(
        IReadOnlyList<PaymentInstructionRequest> instructions, DirectEntrySettings settings) =>
        (
            settings.DescriptionOfEntriesOnFile,
            instructions.Min(instruction => instruction.PaymentDate.Date));
}
