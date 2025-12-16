using CoffeePeek.Shared.Infrastructure.Cache;

namespace CoffeePeek.Shared.Infrastructure.Interfaces.Cache;

public interface IHybridCache
{
    Task<T?> GetOrSetAsync<T>(
        CacheKey cacheKey,
        Func<Task<T?>> factory,
        TimeSpan? distributedTtl = null,
        TimeSpan? memoryTtl = null,
        CancellationToken cancellationToken = default);

    Task SetAsync<T>(
        CacheKey cacheKey,
        T value,
        TimeSpan? distributedTtl = null,
        TimeSpan? memoryTtl = null,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(CacheKey cacheKey, CancellationToken cancellationToken = default);
}