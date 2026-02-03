using CoffeePeek.Shared.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shared.Extensions.Modules;

public static class AuthModule
{
    /// <summary>
    /// Adds user context service that reads user info from headers (set by Gateway)
    /// Use this in downstream services that receive requests through the Gateway
    /// </summary>
    public static IServiceCollection AddHeaderUserContext(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, HeaderUserContext>();
        return services;
    }
}

