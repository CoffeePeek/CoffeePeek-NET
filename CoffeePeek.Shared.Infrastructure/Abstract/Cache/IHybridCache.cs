using CoffeePeek.Shared.Infrastructure.Cache;

namespace CoffeePeek.Shared.Infrastructure.Abstract;

public interface IHybridCache
{
    Task<T?> GetOrSetAsync<T>(
        CacheKey cacheKey,
        Func<Task<T?>> factory,
        TimeSpan? distributedTtl = null,
        TimeSpan? memoryTtl = null,
        CancellationToken ct = default);

    Task SetAsync<T>(
        CacheKey cacheKey,
        T value,
        TimeSpan? distributedTtl = null,
        TimeSpan? memoryTtl = null,
        CancellationToken ct = default);

    Task RemoveAsync(CacheKey cacheKey, CancellationToken ct = default);
    
    Task RemoveByPatternAsync(string pattern);
}