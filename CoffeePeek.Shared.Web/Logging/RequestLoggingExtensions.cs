using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Events;

namespace CoffeePeek.Shared.Web.Logging;

public static class RequestLoggingExtensions
{
    public static bool IsHealthCheckPath(PathString path) =>
        path.StartsWithSegments("/health") || path.StartsWithSegments("/alive");

    public static WebApplication UseCoffeePeekRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.GetLevel = (httpContext, _, ex) =>
            {
                if (ex is not null)
                    return LogEventLevel.Error;

                return IsHealthCheckPath(httpContext.Request.Path)
                    ? LogEventLevel.Verbose
                    : LogEventLevel.Information;
            };
        });

        return app;
    }
}
