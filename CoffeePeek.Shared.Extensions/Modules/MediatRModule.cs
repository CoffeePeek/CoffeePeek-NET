using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shared.Extensions.Modules;

public static class MediatRModule
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddMediatRModule(Assembly assembly)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);
            });

            return services;
        }

        public IServiceCollection AddMediatRModule(Type typeFromAssembly)
        {
            return services.AddMediatRModule(typeFromAssembly.Assembly);
        }
    }
}

