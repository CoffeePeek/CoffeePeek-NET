using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace CoffeePeek.Shared.Extensions.Swagger;

public static class SwaggerExtensions
{
    public static void AddSwagger(this IServiceCollection services, string title, string version)
    {
        services.AddEndpointsApiExplorer();
        
        services.AddSwaggerGen(option =>
        {
            option.OperationFilter<AuthorizeCheckOperationFilter>();

            option.SwaggerDoc(version, new OpenApiInfo
            {
                Title = title,
                Version = version
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
    }
    
}