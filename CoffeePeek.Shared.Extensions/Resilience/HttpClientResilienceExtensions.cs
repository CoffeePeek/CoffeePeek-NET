using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace CoffeePeek.Shared.Extensions.Resilience;

public static class HttpClientResilienceExtensions
{
    private static readonly TimeSpan BreakDuration = TimeSpan.FromSeconds(30);
    private const int ExceptionsBeforeBreak = 5;
    private const int RetryCount = 3;

    public static IHttpClientBuilder AddResiliencePolicies(this IHttpClientBuilder builder, string clientName)
    {
        return builder
            .AddPolicyHandler((sp, _) => BuildRetryPolicy(sp, clientName))
            .AddPolicyHandler((sp, _) => BuildCircuitBreakerPolicy(sp, clientName));
    }

    private static IAsyncPolicy<HttpResponseMessage> BuildRetryPolicy(IServiceProvider sp, string clientName)
    {
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("HttpClientPolicies");

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: RetryCount,
                sleepDurationProvider: retryAttempt =>
                {
                    var baseDelay = Math.Pow(2, retryAttempt);
                    var jitter = Random.Shared.NextDouble() * 0.2;
                    return TimeSpan.FromSeconds(baseDelay + jitter);
                },
                onRetry: (outcome, timespan, retryAttempt, _) =>
                {
                    var statusCode = outcome.Result?.StatusCode;
                    logger.LogWarning(
                        "Retrying HTTP client {ClientName} after {Delay}. Attempt {Attempt}. StatusCode: {StatusCode}.",
                        clientName,
                        timespan,
                        retryAttempt,
                        statusCode);
                });
    }

    private static IAsyncPolicy<HttpResponseMessage> BuildCircuitBreakerPolicy(IServiceProvider sp, string clientName)
    {
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("HttpClientPolicies");

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: ExceptionsBeforeBreak,
                durationOfBreak: BreakDuration,
                onBreak: (outcome, breakDelay) =>
                {
                    var statusCode = outcome.Result?.StatusCode;
                    logger.LogWarning(
                        "Circuit broken for HTTP client {ClientName} for {BreakDuration}. StatusCode: {StatusCode}.",
                        clientName,
                        breakDelay,
                        statusCode);
                },
                onReset: () =>
                {
                    logger.LogInformation("Circuit reset for HTTP client {ClientName}.", clientName);
                });
    }
}

