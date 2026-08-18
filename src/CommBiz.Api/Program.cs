using System.Text.Json;
using CommBiz.Api.Features.BPay;
using CommBiz.Api.Features.DirectEntry;
using CommBiz.Api.Features.Fx;
using CommBiz.Api.Features.Imt;
using CommBiz.Api.Features.PaymentRouting;
using CommBiz.Api.Features.PriorityPayments;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWolverine();
builder.Services.AddOpenApi();
builder.Services.Configure<DirectEntrySettings>(builder.Configuration.GetSection("DirectEntry"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<DirectEntrySettings>>().Value);
builder.Services.Configure<BPaySettings>(builder.Configuration.GetSection("BPay"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<BPaySettings>>().Value);
builder.Services.Configure<ImtSettings>(builder.Configuration.GetSection("Imt"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<ImtSettings>>().Value);
builder.Services.Configure<PriorityPaymentsSettings>(builder.Configuration.GetSection("PriorityPayments"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<PriorityPaymentsSettings>>().Value);
builder.Services.Configure<FxSettings>(builder.Configuration.GetSection("Fx"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<FxSettings>>().Value);

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

// F-015: endpoint is payment-type-agnostic (architecture.md §2) — it reads the raw batch and
// hands it to the Payment Type Router, which decides what slice (if any) to dispatch to.
app.MapPost("/convert", async (JsonElement body, IMessageBus bus) =>
    await PaymentTypeRouter.RouteAndDispatchAsync(body, bus));

app.Run();

// Exposed so WebApplicationFactory<Program> can bootstrap the host in tests.
public partial class Program;
