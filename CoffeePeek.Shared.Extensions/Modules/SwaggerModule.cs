using CoffeePeek.Shared.Extensions.Swagger;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shared.Extensions.Modules;

public static class SwaggerModule
{
    public static IServiceCollection AddSwaggerModule(
        this IServiceCollection services,
        string title,
        string version = "v1")
    {
        services.AddSwagger(title, version);
        return services;
    }
}

