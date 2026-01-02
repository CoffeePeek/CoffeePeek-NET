using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shared.Extensions.Modules;

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
                    .WithOrigins(
                        "http://localhost:5173",
                        "http://127.0.0.1:5173"
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
#else
                if (origins.Length == 0)
                    throw new InvalidOperationException("CORS enabled but ALLOWED_ORIGINS is empty");

                policy
                    .WithOrigins(origins)
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
#endif
            });
        });

        return services;
    }

}