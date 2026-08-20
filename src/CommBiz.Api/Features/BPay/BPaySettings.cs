namespace CommBiz.Api.Features.BPay;

// Static/organisation-level BPay configuration (F-016, confirmed values - see appsettings.json's "BPay"
// section). Bound via builder.Services.Configure<BPaySettings>(...) in Program.cs, mirrors
// DirectEntrySettings' style.
public record BPaySettings
{
    public string FundingAccount { get; init; } = "";

    public string FileNumber { get; init; } = "001";
}
