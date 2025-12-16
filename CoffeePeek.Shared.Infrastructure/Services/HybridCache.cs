using System.Collections.Concurrent;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Interfaces.Cache;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Shared.Infrastructure.Services;

public class HybridCache(
    IMemoryCache memoryCache,
    IRedisService redisService,
    ILogger<HybridCache> logger) : IHybridCache
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public async Task<T?> GetOrSetAsync<T>(
        CacheKey cacheKey,
        Func<Task<T?>> factory,
        TimeSpan? distributedTtl = null,
        TimeSpan? memoryTtl = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cacheKey);
        ArgumentNullException.ThrowIfNull(factory);

        if (memoryCache.TryGetValue(cacheKey.Key, out T? memoryValue))
        {
            return memoryValue;
        }

        var semaphore = _locks.GetOrAdd(cacheKey.Key, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            if (memoryCache.TryGetValue(cacheKey.Key, out memoryValue))
            {
                return memoryValue;
            }

            var redisValue = await redisService.GetAsync<T>(cacheKey);
            if (redisValue is not null)
            {
                StoreInMemory(cacheKey.Key, redisValue, memoryTtl);
                return redisValue;
            }

            var freshValue = await factory();
            if (freshValue is null)
            {
                return default;
            }

            await SetAsync(cacheKey, freshValue, distributedTtl, memoryTtl, cancellationToken);
            return freshValue;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task SetAsync<T>(
        CacheKey cacheKey,
        T value,
        TimeSpan? distributedTtl = null,
        TimeSpan? memoryTtl = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cacheKey);

        StoreInMemory(cacheKey.Key, value, memoryTtl);

        try
        {
            await redisService.SetAsync(cacheKey, value, distributedTtl);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to set distributed cache for key {CacheKey}", cacheKey.Key);
        }
    }

    public async Task RemoveAsync(CacheKey cacheKey, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cacheKey);

        memoryCache.Remove(cacheKey.Key);
        await redisService.RemoveAsync(cacheKey);
    }

    private void StoreInMemory<T>(string key, T value, TimeSpan? memoryTtl)
    {
        var ttl = memoryTtl ?? TimeSpan.FromMinutes(1);
        memoryCache.Set(key, value, ttl);
    }
}