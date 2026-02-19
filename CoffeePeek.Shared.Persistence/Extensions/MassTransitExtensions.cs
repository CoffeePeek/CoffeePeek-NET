using System.Reflection;
using CoffeePeek.Shared.Kernel.Extentions;
using CoffeePeek.Shared.Web;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shared.Persistence.Extensions;

public static class MassTransitExtensions
{
    public static IServiceCollection AddMessaging<TDbContext>(
        this IServiceCollection services,
        Assembly consumersAssembly)
        where TDbContext : DbContext
    {
        var options = services.AddValidateOptions<RabbitMqOptions>();
        
        services.AddMassTransit(x =>
        {
            x.AddConsumers(consumersAssembly);

            x.AddEntityFrameworkOutbox<TDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });
            
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(options.HostName, options.Port, options.VirtualHost, h =>
                {
                    h.Username(options.Username);
                    h.Password(options.Password);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}