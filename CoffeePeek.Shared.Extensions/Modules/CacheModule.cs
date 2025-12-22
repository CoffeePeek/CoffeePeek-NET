using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shared.Extensions.Modules;

public static class CacheModule
{
    public static IServiceCollection AddCacheModule(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.RedisConfigurationOptions();
        services.AddSingleton<IRedisService, RedisService>();
        services.AddSingleton<IHybridCache, HybridCache>();
        services.AddSingleton<ICacheInvalidationStrategy, CacheInvalidationStrategy>();
        
        return services;
    }
}

