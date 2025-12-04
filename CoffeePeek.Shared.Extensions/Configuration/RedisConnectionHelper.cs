using CoffeePeek.Shared.Extensions.Options;

namespace CoffeePeek.Shared.Extensions.Configuration;

public static class RedisConnectionHelper
{
    public static RedisOptions? GetRedisOptions()
    {
        var redisUrl = Environment.GetEnvironmentVariable("REDIS_URL");
        if (!string.IsNullOrEmpty(redisUrl))
        {
            try
            {
                var uri = new Uri(redisUrl);
                var host = uri.Host;
                var port = uri.Port > 0 ? uri.Port : 6379;
                var password = uri.UserInfo.Contains(':') 
                    ? uri.UserInfo.Split(':')[1] 
                    : (uri.UserInfo.StartsWith(":") ? uri.UserInfo.TrimStart(':') : uri.UserInfo);
                
                return new RedisOptions
                {
                    Host = host,
                    Port = port,
                    Password = password
                };
            }
            catch
            {
                //ignore
            }
        }
        
        var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST");
        var redisPort = Environment.GetEnvironmentVariable("REDIS_PORT");
        var redisPassword = Environment.GetEnvironmentVariable("REDIS_PASSWORD");
        
        if (!string.IsNullOrEmpty(redisHost))
        {
            var port = int.TryParse(redisPort, out var p) ? p : 6379;
            return new RedisOptions
            {
                Host = redisHost,
                Port = port,
                Password = redisPassword ?? ""
            };
        }
        
        var redisOptionsHost = Environment.GetEnvironmentVariable("RedisOptions__Host");
        var redisOptionsPort = Environment.GetEnvironmentVariable("RedisOptions__Port");
        var redisOptionsPassword = Environment.GetEnvironmentVariable("RedisOptions__Password");
        
        if (!string.IsNullOrEmpty(redisOptionsHost))
        {
            var port = int.TryParse(redisOptionsPort, out var p) ? p : 6379;
            return new RedisOptions
            {
                Host = redisOptionsHost,
                Port = port,
                Password = redisOptionsPassword ?? ""
            };
        }
        
        return null;
    }
}




