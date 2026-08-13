namespace CommBiz.Api.Features.DirectEntry;

// Header Record mapping (F-007, architecture.md §3/§4 step 5; docs/stash/Direct Entry - File Specification
// CommBiz.md §1): manual field concatenation only, per ADR-004 (no AutoMapper).
public static class DirectEntryHeaderRecordMapper
{
    private const string RecordType = "0";
    private const string ReelSequenceNumber = "01";
    private const string UserFinancialInstitution = "CBA";

    public static string Map(ConvertDirectEntryBatchRequest request) =>
        RecordType +
        new string(' ', 17) +
        ReelSequenceNumber +
        UserFinancialInstitution +
        new string(' ', 7) +
        request.FileName.PadRight(26) +
        request.UserIdentificationNumber.PadLeft(6, '0') +
        request.DescriptionOfEntries.PadRight(12) +
        request.DateToBeProcessed.ToString("ddMMyy") +
        new string(' ', 40);
}
