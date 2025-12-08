using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using CoffeePeek.Shared.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shared.Extensions.Modules;

public static class CacheModule
{
    public static IServiceCollection AddCacheModule(this IServiceCollection services)
    {
        services.RedisConfigurationOptions();
        services.AddScoped<IRedisService, RedisService>();
        
        return services;
    }
}

