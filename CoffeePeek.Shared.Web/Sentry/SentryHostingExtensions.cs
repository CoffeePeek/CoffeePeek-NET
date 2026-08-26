using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Sentry.AspNetCore;

namespace CoffeePeek.Shared.Web.Sentry;

public static class SentryHostingExtensions
{
    /// <summary>
    /// Configures Sentry with structured logs and metrics enabled.
    /// </summary>
    public static IWebHostBuilder UseCoffeePeekSentry(
        this IWebHostBuilder builder,
        Action<SentryAspNetCoreOptions>? configure = null)
    {
        return builder.UseSentry(options =>
        {
            options.EnableLogs = true;
            options.EnableMetrics = true;
            options.SendDefaultPii = false;
            configure?.Invoke(options);
        });
    }
}
