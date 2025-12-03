using CoffeePeek.Photo.Core.Interfaces;
using CoffeePeek.Photo.Infrastructure.Consumers;
using CoffeePeek.Photo.Infrastructure.Options;
using CoffeePeek.Photo.Infrastructure.Storage;
using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Extensions.Options;
using MassTransit;

namespace CoffeePeek.Photo.Api.Configuration;

internal static class InfrastructureModule
{
    public static IServiceCollection LoadInfrastructure(this IServiceCollection services)
    {
        services.AddValidateOptions<DigitalOceanOptions>();
        
        services.AddScoped<IPhotoStorage, S3PhotoStorage>();
        
        ConfigureMassTransit(services);

        return services;
    }

    private static void ConfigureMassTransit(IServiceCollection services)
    {
        var rabbitMqOptions = services.AddValidateOptions<RabbitMqOptions>();
        // Переопределяем опции если они получены из Railway переменных окружения
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
            x.AddConsumer<PhotoUploadRequestedConsumer>();

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
    }
}