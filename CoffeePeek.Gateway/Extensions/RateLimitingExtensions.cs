using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace CoffeePeek.Gateway.Extensions;

public static class RateLimitingExtensions
{
    // Policy names — used in route metadata and middleware
    public const string GlobalPolicy = "global";
    public const string AuthEndpointsPolicy = "auth-endpoints";
    public const string MediaUploadPolicy = "media-upload";

    public static IServiceCollection AddGatewayRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Global sliding-window policy: 200 requests per minute per IP
            options.AddSlidingWindowLimiter(GlobalPolicy, limiterOptions =>
            {
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.SegmentsPerWindow = 6;
                limiterOptions.PermitLimit = 200;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });

            // Strict policy for auth endpoints: 20 requests per minute per IP (brute-force protection)
            options.AddSlidingWindowLimiter(AuthEndpointsPolicy, limiterOptions =>
            {
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.SegmentsPerWindow = 6;
                limiterOptions.PermitLimit = 20;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });

            // Relaxed policy for media uploads: 10 uploads per minute per IP
            options.AddSlidingWindowLimiter(MediaUploadPolicy, limiterOptions =>
            {
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.SegmentsPerWindow = 6;
                limiterOptions.PermitLimit = 10;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });

            // Partition by real client IP — Railway proxy injects X-Forwarded-For with real client IP as first entry.
            // Fallback: X-Real-IP → RemoteIpAddress → "unknown". Split on comma handles multi-proxy chains.
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var clientIp =
                    context.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim()
                    ?? context.Request.Headers["X-Real-IP"].FirstOrDefault()
                    ?? context.Connection.RemoteIpAddress?.ToString()
                    ?? "unknown";
                return RateLimitPartition.GetSlidingWindowLimiter(clientIp, _ => new SlidingWindowRateLimiterOptions
                {
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 6,
                    PermitLimit = 300,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                });
            });
        });

        return services;
    }
}
