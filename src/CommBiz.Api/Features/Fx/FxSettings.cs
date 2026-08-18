namespace CommBiz.Api.Features.Fx;

// Static/organisation-level FX configuration (F-023, confirmed values - see appsettings.json's "Fx"
// section). Bound via builder.Services.Configure<FxSettings>(...) in Program.cs, mirrors
// PriorityPaymentsSettings style.
public record FxSettings
{
    public string SellInstruction { get; init; } = "";

    public string BuyInstruction { get; init; } = "";

    public string BuyPaymentDetails { get; init; } = "";

    public string SellPaymentDetails { get; init; } = "";
}
