using CommBiz.Api.Features.Diagnostics;
using CommBiz.Api.Features.DirectEntry;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWolverine();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

// Diagnostic-only endpoint proving the Wolverine dispatch path (F-002); not a real feature.
app.MapPost("/diagnostics/ping", async (PingCommand command, IMessageBus bus) =>
    Results.Ok(await bus.InvokeAsync<PingResult>(command)));

app.MapPost("/direct-entry/convert", async (ConvertDirectEntryBatchRequest request, IMessageBus bus) =>
    Results.Ok(await bus.InvokeAsync<ConvertDirectEntryBatchResponse>(new ConvertDirectEntryBatchCommand(request))));

app.Run();

// Exposed so WebApplicationFactory<Program> can bootstrap the host in tests.
public partial class Program;
