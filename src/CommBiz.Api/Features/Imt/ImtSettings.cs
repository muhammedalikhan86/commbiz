namespace CommBiz.Api.Features.Imt;

// Static/organisation-level IMT configuration (F-017, confirmed values - see appsettings.json's "Imt"
// section). Bound via builder.Services.Configure<ImtSettings>(...) in Program.cs, mirrors
// DirectEntrySettings/BPaySettings style. DebitAccountName is descriptive/documentation only - there
// is no debit-side account-name field in the CBA file spec, so it is never written to the output.
public record ImtSettings
{
    public string DebitAccountBsb { get; init; } = "";

    public string DebitAccountNumber { get; init; } = "";

    public string DebitAccountName { get; init; } = "";
}
