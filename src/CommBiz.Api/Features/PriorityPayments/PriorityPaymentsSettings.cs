namespace CommBiz.Api.Features.PriorityPayments;

// Static/organisation-level Priority Payments configuration (F-018, confirmed values - see
// appsettings.json's "PriorityPayments" section). Bound via
// builder.Services.Configure<PriorityPaymentsSettings>(...) in Program.cs, mirrors ImtSettings style.
// DebitAccountName is descriptive/documentation only - never written to the output.
public record PriorityPaymentsSettings
{
    public string DebitAccountBsb { get; init; } = "";

    public string DebitAccountNumber { get; init; } = "";

    public string DebitAccountName { get; init; } = "";
}
