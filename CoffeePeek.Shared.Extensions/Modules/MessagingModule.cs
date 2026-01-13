using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Infrastructure.Options;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shared.Extensions.Modules;

public static class MessagingModule
{
    public static void AddMessagingModule(this IServiceCollection services,
        Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        var rabbitMqOptions = services.AddValidateOptions<RabbitMqOptions>();

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            configureConsumers?.Invoke(x);
            
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqOptions.HostName, rabbitMqOptions.Port, rabbitMqOptions.VirtualHost, h =>
                {
                    h.Username(rabbitMqOptions.Username);
                    h.Password(rabbitMqOptions.Password);
                });

                cfg.ConfigureEndpoints(context);
            });
        });
    }
}