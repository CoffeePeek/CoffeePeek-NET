using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shared.Extensions.Modules;

public static class MediatRModule
{
    public static IServiceCollection AddMediatRModule(
        this IServiceCollection services,
        Assembly assembly)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        return services;
    }

    public static IServiceCollection AddMediatRModule(
        this IServiceCollection services,
        Type typeFromAssembly)
    {
        return services.AddMediatRModule(typeFromAssembly.Assembly);
    }
}

