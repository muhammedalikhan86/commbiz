using CommBiz.Api.Features.DirectEntry;
using Microsoft.Extensions.Options;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWolverine();
builder.Services.Configure<DirectEntrySettings>(builder.Configuration.GetSection("DirectEntry"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<DirectEntrySettings>>().Value);

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.MapPost("/convert", async (List<PaymentInstructionRequest> instructions, IMessageBus bus) =>
    Results.Ok(await bus.InvokeAsync<ConvertDirectEntryBatchResponse>(new ConvertDirectEntryBatchCommand(instructions))));

app.Run();

// Exposed so WebApplicationFactory<Program> can bootstrap the host in tests.
public partial class Program;
