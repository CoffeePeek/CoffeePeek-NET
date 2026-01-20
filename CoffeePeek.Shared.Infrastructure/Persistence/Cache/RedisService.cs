using System.Text.Json;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using StackExchange.Redis;

namespace CoffeePeek.Shared.Infrastructure.Persistence;

public class RedisService(IConnectionMultiplexer redis) : IRedisService
{
    private readonly IDatabase _db = redis.GetDatabase();

    private readonly IServer _server = redis.GetServer(redis.GetEndPoints().FirstOrDefault() ??
                                                       throw new InvalidOperationException(
                                                           "No Redis endpoints configured"));

    public async Task<T?> GetAsync<T>(CacheKey cacheKey)
    {
        try
        {
            string? value = await _db.StringGetAsync(cacheKey.Key);
            return string.IsNullOrEmpty(value) ? default : JsonSerializer.Deserialize<T>(value);
        }
        catch (RedisConnectionException)
        {
            return default;
        }
        catch (Exception)
        {
            return default;
        }
    }

    public async Task<T?> GetAsync<T>(
        CacheKey cacheKey, 
        Func<Task<T>> factory, 
        TimeSpan? expiration = null)
    {
        try
        {
            string? cachedValue = await _db.StringGetAsync(cacheKey.Key);
        
            if (!string.IsNullOrEmpty(cachedValue))
            {
                return JsonSerializer.Deserialize<T>(cachedValue);
            }

            var result = await factory();

            if (result != null)
            {
                await _db.StringSetAsync(
                    cacheKey.Key, 
                    JsonSerializer.Serialize(result), 
                    expiration ?? TimeSpan.FromMinutes(10));
            }

            return result;
        }
        catch (Exception ex)
        {
            return await factory();
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            await _db.StringSetAsync(key, JsonSerializer.Serialize(value), new Expiration(expiry ?? TimeSpan.FromHours(1)));
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

    public async Task RemoveByPattern(string pattern)
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