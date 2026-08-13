namespace CommBiz.Api.Features.DirectEntry;

// Static/organisation-level Direct Entry configuration (confirmed values, see appsettings.json's
// "DirectEntry" section). Bound via builder.Services.Configure<DirectEntrySettings>(...) in Program.cs.
public record DirectEntrySettings
{
    public string InstitutionCode { get; init; } = "";

    public string UserIdentificationNumber { get; init; } = "";

    public string NameOfUserSupplyingFile { get; init; } = "";

    public string Title { get; init; } = "";

    public string DescriptionOfEntriesOnFile { get; init; } = "";

    public string LodgementReferenceDetails { get; init; } = "";

    public string TraceAccountBsb { get; init; } = "";

    public string TraceAccountAccNo { get; init; } = "";

    public string NameOfRemitter { get; init; } = "";

    public string AmountOfWithholdingTax { get; init; } = "";

    public string TransactionCode { get; init; } = "";
}
