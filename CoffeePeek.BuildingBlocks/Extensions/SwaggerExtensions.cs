using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace CoffeePeek.BuildingBlocks.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(option =>
        {
            option.OperationFilter<AuthorizeCheckOperationFilter>();

            option.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "CoffeePeek API",
                Version = "v1"
            });
            option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
        });

        return services;
    }
}