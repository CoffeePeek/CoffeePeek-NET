using CoffeePeek.Shared.Infrastructure.Cache;

namespace CoffeePeek.Shared.Infrastructure.Interfaces.Redis;

public interface IRedisService
{
    Task<T?> GetAsync<T>(CacheKey cacheKey);
    Task SetAsync<T>(CacheKey cacheKey, T value, TimeSpan? customTtl = null);
    Task RemoveAsync(CacheKey cacheKey);
    Task<bool> ExistsAsync(CacheKey cacheKey);
    
    Task RemoveByPatternAsync(string pattern);
}