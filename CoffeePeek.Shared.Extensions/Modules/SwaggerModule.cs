using CoffeePeek.Shared.Extensions.Swagger;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shared.Extensions.Modules;

public static class SwaggerModule
{
    public static void AddSwaggerModule(this IServiceCollection services, string title)
    {
        services.AddSwagger(title);
    }
}