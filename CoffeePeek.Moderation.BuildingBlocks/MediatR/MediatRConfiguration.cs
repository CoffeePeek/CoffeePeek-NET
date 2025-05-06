using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Moderation.BuildingBlocks.MediatR;

public static class MediatRConfiguration
{
    public static IServiceCollection AddMediatR(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });
        
        return services;
    }
}