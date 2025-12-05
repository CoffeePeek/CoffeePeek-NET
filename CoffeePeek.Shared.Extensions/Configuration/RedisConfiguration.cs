using System.Net;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace CoffeePeek.Shared.Extensions.Configuration;

public static class RedisConfiguration
{
    public static void RedisConfigurationOptions(this IServiceCollection services)
    {
        var options = services.AddValidateOptions<Options.RedisOptions>();
        
        // Переопределяем опции если они получены из Railway переменных окружения
        var railwayOptions = RedisConnectionHelper.GetRedisOptions();
        if (railwayOptions != null)
        {
            options.Host = railwayOptions.Host;
            options.Port = railwayOptions.Port;
            options.Password = railwayOptions.Password;
        }

        var redisConfig = new ConfigurationOptions
        {
            EndPoints = { new DnsEndPoint(options.Host, options.Port) },
            AbortOnConnectFail = false,
            Ssl = false,
            Password = options.Password,
            User = "default"
        };

        services.AddSingleton<IConnectionMultiplexer>(_ => 
            ConnectionMultiplexer.Connect(redisConfig));
    }
}