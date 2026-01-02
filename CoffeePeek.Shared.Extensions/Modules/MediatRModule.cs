using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shared.Extensions.Modules;

public static class MediatRModule
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddMediatRModule(params Assembly[] assemblies)
        {
            services.AddMediatR(cfg => { cfg.RegisterServicesFromAssemblies(assemblies); });

            return services;
        }

        public IServiceCollection AddMediatRModule(params Type[] typesFromAssemblies)
        {
            var assemblies = typesFromAssemblies.Select(t => t.Assembly).ToArray();
            return services.AddMediatRModule(assemblies);
        }
    }
}