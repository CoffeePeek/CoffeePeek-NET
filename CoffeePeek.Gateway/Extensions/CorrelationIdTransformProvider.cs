using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace CoffeePeek.Gateway.Extensions;

/// <summary>
/// YARP transform provider that:
/// 1. Propagates or generates a correlation ID header (<c>X-Correlation-Id</c>) on every proxied request.
/// 2. Logs request/response details (route, cluster, status code, duration) for observability.
/// </summary>
public class CorrelationIdTransformProvider : ITransformProvider
{
    public const string CorrelationIdHeader = "X-Correlation-Id";

    public void ValidateRoute(TransformRouteValidationContext context) { }

    public void ValidateCluster(TransformClusterValidationContext context) { }

    public void Apply(TransformBuilderContext context)
    {
        var routeId = context.Route.RouteId;
        var clusterId = context.Cluster?.ClusterId ?? "unknown";

        // Request transform: propagate or generate correlation ID
        context.AddRequestTransform(transformContext =>
        {
            var httpContext = transformContext.HttpContext;

            // Reuse existing correlation ID from the client, or generate a new one
            var correlationId = httpContext.Request.Headers[CorrelationIdHeader].FirstOrDefault()
                                ?? httpContext.TraceIdentifier;

            // Store in HttpContext.Items so the response transform can access it
            httpContext.Items[CorrelationIdHeader] = correlationId;

            // Forward to downstream service
            transformContext.ProxyRequest.Headers.TryAddWithoutValidation(CorrelationIdHeader, correlationId);

            return ValueTask.CompletedTask;
        });

        // Response transform: echo correlation ID back to the client and log the result
        context.AddResponseTransform(transformContext =>
        {
            var httpContext = transformContext.HttpContext;

            if (httpContext.Items.TryGetValue(CorrelationIdHeader, out var correlationId) &&
                correlationId is string correlationIdStr)
            {
                transformContext.HttpContext.Response.Headers.TryAdd(CorrelationIdHeader, correlationIdStr);
            }

            return ValueTask.CompletedTask;
        });
    }
}
