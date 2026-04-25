using System.Net;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Abstract;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline;

/// <summary>Retries the inner pipeline on transient failures (limited attempts).</summary>
public sealed class ResilienceBehavior : IHttpPipelineStep
{
    private const int MaxAttempts = 3;

    public async Task<HttpResult> Execute(
        HttpCommand command,
        Func<HttpCommand, Task<HttpResult>> next,
        CancellationToken ct)
    {
        Exception? lastException = null;

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                var result = await next(command);
                if (ShouldRetry(result) && attempt < MaxAttempts)
                {
                    await DelayAsync(attempt, ct);
                    continue;
                }

                return result;
            }
            catch (HttpRequestException ex) when (attempt < MaxAttempts)
            {
                lastException = ex;
                await DelayAsync(attempt, ct);
            }
            catch (TaskCanceledException ex) when (!ct.IsCancellationRequested && attempt < MaxAttempts)
            {
                lastException = ex;
                await DelayAsync(attempt, ct);
            }
        }

        return new HttpResult
        {
            StatusCode = HttpStatusCode.ServiceUnavailable,
            IsSuccess = false,
            RawBody = string.Empty,
            ErrorMessage = lastException?.Message ?? "Request failed after retries."
        };
    }

    private static bool ShouldRetry(HttpResult result) =>
        result.StatusCode is HttpStatusCode.RequestTimeout
            or HttpStatusCode.TooManyRequests
            or HttpStatusCode.InternalServerError
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout;

    private static Task DelayAsync(int attempt, CancellationToken ct) =>
        Task.Delay(TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt - 1)), ct);
}
