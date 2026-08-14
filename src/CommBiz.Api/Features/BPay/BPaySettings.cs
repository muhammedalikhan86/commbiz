namespace CommBiz.Api.Features.BPay;

// Static/organisation-level BPay configuration (see appsettings.json's "BPay" section - placeholder
// values, not yet confirmed, see F-016 Assumptions). Bound via builder.Services.Configure<BPaySettings>(...)
// in Program.cs, mirrors DirectEntrySettings' style.
public record BPaySettings
{
    public string FundingAccount { get; init; } = "";

    public string FileNumber { get; init; } = "001";
}
