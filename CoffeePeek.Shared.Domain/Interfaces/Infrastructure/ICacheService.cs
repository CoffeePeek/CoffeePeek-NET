namespace CoffeePeek.Shared.Domain.Interfaces.Infrastructure;

public interface ICacheService
{
    Task<T?> GetAsync<T>(CacheKey cacheKey, CancellationToken cancellationToken = default);
    Task<T?> GetAsync<T>(
        CacheKey cacheKey,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);
    Task SetAsync<T>(CacheKey cacheKey, T value, TimeSpan? customTtl = null);
    Task RemoveAsync(CacheKey cacheKey);
    Task<bool> ExistsAsync(CacheKey cacheKey);
    
    Task RemoveByPattern(string pattern, CancellationToken cancellationToken = default);
}
