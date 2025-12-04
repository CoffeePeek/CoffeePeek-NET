using CoffeePeek.Shared.Extensions.Options;

namespace CoffeePeek.Shared.Extensions.Configuration;

public static class RabbitMqConnectionHelper
{
    public static RabbitMqOptions? GetRabbitMqOptions()
    {
        var rabbitMqUrl = Environment.GetEnvironmentVariable("RABBITMQ_URL");
        if (!string.IsNullOrEmpty(rabbitMqUrl))
        {
            try
            {
                var uri = new Uri(rabbitMqUrl);
                var host = uri.Host;
                var port = uri.Port > 0 ? (ushort)uri.Port : (ushort)5672;
                var user = uri.UserInfo.Split(':')[0];
                var password = uri.UserInfo.Split(':').Length > 1 ? uri.UserInfo.Split(':')[1] : "";
                
                return new RabbitMqOptions
                {
                    HostName = host,
                    Port = port,
                    Username = user,
                    Password = password
                };
            }
            catch
            {
                //ignore
            }
        }
        
        var rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
        var rabbitMqPort = Environment.GetEnvironmentVariable("RABBITMQ_PORT");
        var rabbitMqUser = Environment.GetEnvironmentVariable("RABBITMQ_USER");
        var rabbitMqPassword = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD");
        
        if (!string.IsNullOrEmpty(rabbitMqHost) && !string.IsNullOrEmpty(rabbitMqUser))
        {
            var port = ushort.TryParse(rabbitMqPort, out var p) ? p : (ushort)5672;
            return new RabbitMqOptions
            {
                HostName = rabbitMqHost,
                Port = port,
                Username = rabbitMqUser,
                Password = rabbitMqPassword ?? ""
            };
        }
        
        var rabbitMqOptionsHost = Environment.GetEnvironmentVariable("RabbitMqOptions__HostName");
        var rabbitMqOptionsPort = Environment.GetEnvironmentVariable("RabbitMqOptions__Port");
        var rabbitMqOptionsUser = Environment.GetEnvironmentVariable("RabbitMqOptions__Username");
        var rabbitMqOptionsPassword = Environment.GetEnvironmentVariable("RabbitMqOptions__Password");
        
        if (!string.IsNullOrEmpty(rabbitMqOptionsHost) && !string.IsNullOrEmpty(rabbitMqOptionsUser))
        {
            var port = ushort.TryParse(rabbitMqOptionsPort, out var p) ? p : (ushort)5672;
            return new RabbitMqOptions
            {
                HostName = rabbitMqOptionsHost,
                Port = port,
                Username = rabbitMqOptionsUser,
                Password = rabbitMqOptionsPassword ?? ""
            };
        }
        
        return null;
    }
}




