using System.Diagnostics;
using System.Text.RegularExpressions;
using CoffeePeek.Shared.Web.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Sentry;

namespace CoffeePeek.Shared.Web.Sentry;

public static partial class SentryRequestMetrics
{
    public const string RequestCounterName = "http.request";
    public const string RequestDurationName = "http.request.duration";

    /// <summary>
    /// Records request popularity (count) and latency (distribution) to Sentry Metrics.
    /// Prefer low-cardinality route templates over raw paths with ids.
    /// </summary>
    public static void Record(HttpContext context, long elapsedMs, string? routeOverride = null)
    {
        if (!SentrySdk.IsEnabled)
            return;

        var method = context.Request.Method;
        var status = context.Response.StatusCode.ToString();
        var route = routeOverride ?? ResolveRoute(context);

        var tags = new Dictionary<string, object>
        {
            ["method"] = method,
            ["route"] = route,
            ["status"] = status
        };

        SentrySdk.Metrics.EmitCounter(RequestCounterName, 1, tags);
        SentrySdk.Metrics.EmitDistribution(
            RequestDurationName,
            elapsedMs,
            MeasurementUnit.Duration.Millisecond,
            tags);
    }

    public static string ResolveRoute(HttpContext context)
    {
        if (context.GetEndpoint() is RouteEndpoint routeEndpoint
            && !string.IsNullOrWhiteSpace(routeEndpoint.RoutePattern.RawText))
        {
            return routeEndpoint.RoutePattern.RawText!;
        }

        return NormalizePath(context.Request.Path.Value ?? "/");
    }

    public static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "/";

        var normalized = GuidPathRegex().Replace(path, "/{id}");
        normalized = NumericSegmentRegex().Replace(normalized, "/{id}");
        return normalized;
    }

    [GeneratedRegex(@"/[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}(?=/|$)", RegexOptions.CultureInvariant)]
    private static partial Regex GuidPathRegex();

    [GeneratedRegex(@"/\d+(?=/|$)", RegexOptions.CultureInvariant)]
    private static partial Regex NumericSegmentRegex();
}

/// <summary>
/// Emits Sentry metrics for each non-health HTTP request (count + duration).
/// </summary>
public sealed class SentryRequestMetricsMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (RequestLoggingExtensions.IsHealthCheckPath(context.Request.Path))
        {
            await next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            SentryRequestMetrics.Record(context, stopwatch.ElapsedMilliseconds);
        }
    }
}

public static class SentryRequestMetricsMiddlewareExtensions
{
    public static IApplicationBuilder UseSentryRequestMetrics(this IApplicationBuilder app) =>
        app.UseMiddleware<SentryRequestMetricsMiddleware>();
}
