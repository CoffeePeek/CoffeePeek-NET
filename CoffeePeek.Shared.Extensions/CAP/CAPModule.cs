using CoffeePeek.Shared.Infrastructure.Options;
using CoffeePeek.Shared.Kernel.Extentions;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shared.Extensions.CAP;

public static class CAPModule
{
    private const int SucceedMessageExpiredAfterSecond = 86400; // 24 hours
    private const int FailedMessageExpiredAfterSecond = 604800; // 7 days
    private const int CollectorCleaningIntervalSecond = 3600;   // 1hour
    
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCapModule(string connectionString, string groupName)
        {
            var rabbitMqOptions = services.AddValidateOptions<RabbitMqOptions>();

            services.AddCap(options =>
            {
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

                options.SucceedMessageExpiredAfter = SucceedMessageExpiredAfterSecond;

                options.FailedMessageExpiredAfter = FailedMessageExpiredAfterSecond;

                options.CollectorCleaningInterval = CollectorCleaningIntervalSecond;
            });
        

            return services;
        }
    }
}