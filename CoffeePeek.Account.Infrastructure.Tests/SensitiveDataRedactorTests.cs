using CoffeePeek.Shared.Web.Logging;
using FluentAssertions;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace CoffeePeek.Account.Infrastructure.Tests;

public class SensitiveDataRedactorTests
{
    [Fact]
    public void Redact_RemovesAccessTokenFromYarpDestinationUrl()
    {
        const string jwt =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0In0.abc";
        var url =
            $"http://account/realtime/session?id=abc123&access_token={jwt}";

        var redacted = SensitiveDataRedactor.Redact(url);

        redacted.Should().Contain("access_token=[REDACTED]");
        redacted.Should().Contain("id=abc123");
        redacted.Should().NotContain("eyJ");
        redacted.Should().NotContain(jwt);
    }

    [Fact]
    public void Redact_RemovesBareJwtAndBearerHeader()
    {
        var text = "Authorization Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIn0.sig";

        var redacted = SensitiveDataRedactor.Redact(text);

        redacted.Should().Contain("Bearer [REDACTED]");
        redacted.Should().NotContain("eyJ");
    }

    [Fact]
    public void Enricher_RedactsStructuredLogProperties()
    {
        const string destination =
            "http://account/realtime/session?id=abc&access_token=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIn0.sig";
        var sink = new CollectingSink();

        using var logger = new LoggerConfiguration()
            .Enrich.With<SensitiveDataRedactingEnricher>()
            .WriteTo.Sink(sink)
            .CreateLogger();

        logger.Information("Proxying to {Destination}", destination);

        sink.Events.Should().ContainSingle();
        var rendered = sink.Events[0].RenderMessage();
        rendered.Should().Contain("access_token=[REDACTED]");
        rendered.Should().NotContain("eyJ");
        rendered.Should().Contain("id=abc");
    }

    private sealed class CollectingSink : ILogEventSink
    {
        public List<LogEvent> Events { get; } = [];

        public void Emit(LogEvent logEvent) => Events.Add(logEvent);
    }
}
