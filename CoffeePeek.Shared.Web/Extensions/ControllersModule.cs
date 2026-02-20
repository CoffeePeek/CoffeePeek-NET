using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shared.Web.Extensions;

public static class ControllersModule
{
    public static IServiceCollection AddControllersModule(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        
        return services;
    }
}

