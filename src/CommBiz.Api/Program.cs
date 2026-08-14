using System.Text.Json;
using CommBiz.Api.Features.BPay;
using CommBiz.Api.Features.DirectEntry;
using CommBiz.Api.Features.PaymentRouting;
using Microsoft.Extensions.Options;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWolverine();
builder.Services.Configure<DirectEntrySettings>(builder.Configuration.GetSection("DirectEntry"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<DirectEntrySettings>>().Value);
builder.Services.Configure<BPaySettings>(builder.Configuration.GetSection("BPay"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<BPaySettings>>().Value);

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

// F-015: endpoint is payment-type-agnostic (architecture.md §2) — it reads the raw batch and
// hands it to the Payment Type Router, which decides what slice (if any) to dispatch to.
app.MapPost("/convert", async (JsonElement body, IMessageBus bus) =>
    await CommBiz.Api.Features.PaymentRouting.PaymentTypeRouter.RouteAndDispatchAsync(body, bus));

app.Run();

// Exposed so WebApplicationFactory<Program> can bootstrap the host in tests.
public partial class Program;
