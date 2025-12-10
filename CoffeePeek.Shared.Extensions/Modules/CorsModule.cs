using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shared.Extensions.Modules;

public static class CorsModule
{
    public static IServiceCollection AddCorsModule(this IServiceCollection services)
    {
        var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS");
        if (!string.IsNullOrEmpty(allowedOrigins))
        {
            var origins = allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(origins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });
        }

        return services;
    }

    public static bool IsCorsEnabled()
    {
        return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ALLOWED_ORIGINS"));
    }
}

