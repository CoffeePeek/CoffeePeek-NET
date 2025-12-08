using System.Text.Json;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using StackExchange.Redis;

namespace CoffeePeek.Shared.Infrastructure.Services;

public class RedisService(IConnectionMultiplexer redis) : IRedisService
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            string? value = await _db.StringGetAsync(key);
            return (string.IsNullOrEmpty(value) ? default : JsonSerializer.Deserialize<T>(value)!)!;
        }
        catch (RedisConnectionException)
        {
            // Redis недоступен, возвращаем null для использования fallback (БД)
            return default;
        }
        catch (Exception)
        {
            // Другие ошибки Redis, возвращаем null
            return default;
        }
    }
    
    public async Task<(bool success, T value)> TryGetAsync<T>(string key)
    {
        try
        {
            string? redisValue = await _db.StringGetAsync(key);

            if (string.IsNullOrEmpty(redisValue))
            {
                return (false, default)!;
            }

            var value = JsonSerializer.Deserialize<T>(redisValue);
            return (value != null, value)!;
        }
        catch (JsonException)
        {
            return (false, default)!;
        }
        catch (RedisConnectionException)
        {
            return (false, default)!;
        }
        catch (Exception)
        {
            return (false, default)!;
        }
    }
    
    public async Task<T> GetAsyncById<T>(string id)
    {
        try
        {
            var key = $"{typeof(T).Name}-{id}";
            
            string? value = await _db.StringGetAsync(key);
            
            return (string.IsNullOrEmpty(value) ? default : JsonSerializer.Deserialize<T>(value))!;
        }
        catch (RedisConnectionException)
        {
            // Redis недоступен, возвращаем null для использования fallback (БД)
            return default!;
        }
        catch (Exception)
        {
            // Другие ошибки Redis, возвращаем null
            return default!;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            await _db.StringSetAsync(key, JsonSerializer.Serialize(value), new Expiration(expiry ?? TimeSpan.FromDays(1)));
        }
        catch (RedisConnectionException)
        {
            // Redis недоступен, игнорируем ошибку (кэш не критичен)
        }
        catch (Exception)
        {
            // Другие ошибки Redis, игнорируем
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _db.KeyDeleteAsync(key);
        }
        catch (RedisConnectionException)
        {
            // Redis недоступен, игнорируем ошибку
        }
        catch (Exception)
        {
            // Другие ошибки Redis, игнорируем
        }
    }
}