using CoffeePeek.Shared.Infrastructure.Cache;

namespace CoffeePeek.Shared.Infrastructure.Abstract;

public interface IRedisService
{
    Task<T?> GetAsync<T>(CacheKey cacheKey);
    Task SetAsync<T>(CacheKey cacheKey, T value, TimeSpan? customTtl = null);
    Task RemoveAsync(CacheKey cacheKey);
    Task<bool> ExistsAsync(CacheKey cacheKey);
    
    Task RemoveByPatternAsync(string pattern);
}