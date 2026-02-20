namespace CoffeePeek.Shared.Domain.Interfaces.Infrastructure;

public interface ICacheService
{
    Task<T?> GetAsync<T>(CacheKey cacheKey);
    Task<T?> GetAsync<T>(CacheKey cacheKey, Func<Task<T>> factory, TimeSpan? expiration = null);
    Task SetAsync<T>(CacheKey cacheKey, T value, TimeSpan? customTtl = null);
    Task RemoveAsync(CacheKey cacheKey);
    Task<bool> ExistsAsync(CacheKey cacheKey);
    
    Task RemoveByPattern(string pattern);
}