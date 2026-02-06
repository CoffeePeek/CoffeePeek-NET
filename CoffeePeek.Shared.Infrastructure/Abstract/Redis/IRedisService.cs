using CoffeePeek.Shared.Infrastructure.Persistence;

namespace CoffeePeek.Shared.Infrastructure.Abstract;

public interface IRedisService
{
    Task<T?> GetAsync<T>(CacheKey cacheKey);
    Task<T?> GetAsync<T>(CacheKey cacheKey, Func<Task<T>> factory, TimeSpan? expiration = null);
    Task SetAsync<T>(CacheKey cacheKey, T value, TimeSpan? customTtl = null);
    Task RemoveAsync(CacheKey cacheKey);
    Task<bool> ExistsAsync(CacheKey cacheKey);
    
    Task RemoveByPattern(string pattern);
}