namespace CoffeePeek.Shared.Infrastructure.Interfaces.Redis;

public interface IRedisService
{
    Task<T?> GetAsync<T>(string key);
    Task<T?> GetAsyncById<T>(string id);
    Task<(bool success, T value)> TryGetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task RemoveAsync(string key);
}