namespace CommBiz.Api.Features.Diagnostics;

// Diagnostic-only plumbing proving the Wolverine dispatch path (F-002); not a real feature.
public record PingCommand(string Message);

public record PingResult(string Message, DateTimeOffset RespondedAtUtc);

public static class PingHandler
{
    public static PingResult Handle(PingCommand command) =>
        new(command.Message, DateTimeOffset.UtcNow);
}
