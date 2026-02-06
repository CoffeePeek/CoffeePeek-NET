using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shared.Extensions.Modules;

public static class CAPModule
{
    /// <param name="services">Service collection</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds DOTNET.CAP with PostgreSQL storage, RabbitMQ transport, and Redis Streams.
        /// </summary>
        /// <param name="connectionString">PostgreSQL connection string for CAP storage</param>
        /// <param name="groupName">Unique group name for this service (e.g., "account-service", "shops-service")</param>
        public IServiceCollection AddCapModule(string connectionString, string groupName)
        {
            var rabbitMqOptions = services.AddValidateOptions<RabbitMqOptions>();

            services.AddCap(options =>
            {
                options.UseDashboard();

                options.UsePostgreSql(connectionString);

                options.UseRabbitMQ(rmq =>
                {
                    rmq.HostName = rabbitMqOptions.HostName;
                    rmq.Port = rabbitMqOptions.Port;
                    rmq.UserName = rabbitMqOptions.Username;
                    rmq.Password = rabbitMqOptions.Password;
                    rmq.VirtualHost = rabbitMqOptions.VirtualHost;
                });

                options.DefaultGroupName = groupName;
                options.FailedRetryCount = 3;
                options.FailedRetryInterval = 60;
    
                options.SucceedMessageExpiredAfter = 24 * 60 * 60; // 86400 секунд
    
                options.FailedMessageExpiredAfter = 7 * 24 * 60 * 60; // 604800 секунд
    
                options.CollectorCleaningInterval = 60 * 60; // 3600 секунд
            });
        

            return services;
        }
    }
}