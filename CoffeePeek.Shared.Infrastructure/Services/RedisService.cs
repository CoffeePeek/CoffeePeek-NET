using System.Text.Json;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using StackExchange.Redis;

namespace CoffeePeek.Shared.Infrastructure.Services;

public class RedisService(IConnectionMultiplexer redis) : IRedisService
{
    private readonly IDatabase _db = redis.GetDatabase();
    private readonly IServer _server = redis.GetServer(redis.GetEndPoints().First());
    
    public async Task<T?> GetAsync<T>(CacheKey cacheKey)
    {
        try
        {
            string? value = await _db.StringGetAsync(cacheKey.Key);
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

    public async Task SetAsync<T>(CacheKey cacheKey, T value, TimeSpan? customTtl = null)
    {
        try
        {
            var ttl = customTtl ?? cacheKey.DefaultTtl ?? TimeSpan.FromDays(1);
            await _db.StringSetAsync(cacheKey.Key, JsonSerializer.Serialize(value), new Expiration(ttl));
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
    
    public async Task RemoveAsync(CacheKey cacheKey)
    {
        try
        {
            await _db.KeyDeleteAsync(cacheKey.Key);
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
    
    public async Task<bool> ExistsAsync(CacheKey cacheKey)
    {
        try
        {
            return await _db.KeyExistsAsync(cacheKey.Key);
        }
        catch (RedisConnectionException)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            var keys = _server.Keys(pattern: pattern).ToArray();
            if (keys.Length > 0)
            {
                await _db.KeyDeleteAsync(keys);
            }
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