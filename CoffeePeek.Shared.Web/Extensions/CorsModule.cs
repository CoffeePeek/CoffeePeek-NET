using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shared.Web.Extensions;

public static class CorsModule
{
    public static IServiceCollection AddCorsModule(this IServiceCollection services)
    {
        var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS");
        var origins = string.IsNullOrWhiteSpace(allowedOrigins)
            ? []
            : allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
#if DEBUG
                policy
                    .SetIsOriginAllowed(origin => 
                    {
                        var host = new Uri(origin).Host;
                        return host == "localhost" || 
                               host == "127.0.0.1" || 
                               host.StartsWith("192.168.") ||
                               host.StartsWith("10.") ||
                               host.EndsWith(".local");
                    })
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
#else
                if (origins.Length == 0)
                    throw new InvalidOperationException("CORS enabled but ALLOWED_ORIGINS is empty");

                policy
                    .WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
#endif
            });
        });

        return services;
    }

}