using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Extensions.Options;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shared.Extensions.Modules;

public static class MessagingModule
{
    public static IServiceCollection AddMessagingModule(
        this IServiceCollection services,
        Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        var rabbitMqOptions = services.AddValidateOptions<RabbitMqOptions>();
        var railwayRabbitMqOptions = RabbitMqConnectionHelper.GetRabbitMqOptions();
        
        if (railwayRabbitMqOptions != null)
        {
            rabbitMqOptions.HostName = railwayRabbitMqOptions.HostName;
            rabbitMqOptions.Port = railwayRabbitMqOptions.Port;
            rabbitMqOptions.Username = railwayRabbitMqOptions.Username;
            rabbitMqOptions.Password = railwayRabbitMqOptions.Password;
        }

        services.AddMassTransit(x =>
        {
            configureConsumers?.Invoke(x);
            
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqOptions.HostName, rabbitMqOptions.Port, "/", h =>
                {
                    h.Username(rabbitMqOptions.Username);
                    h.Password(rabbitMqOptions.Password);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}

