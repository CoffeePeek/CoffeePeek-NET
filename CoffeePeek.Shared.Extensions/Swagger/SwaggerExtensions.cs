using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using CoffeePeek.Shared.Infrastructure.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace CoffeePeek.Shared.Extensions.Swagger;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwagger(this IServiceCollection services, string apiTitle)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = ApiVersions.Default;
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = new HeaderApiVersionReader(ApiVersions.VersionHeader);
        })
        .AddMvc()
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = ApiVersions.GroupNameFormat;
        });

        services.AddSwaggerGen(option =>
        {
            option.EnableAnnotations();
            
            var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

            foreach (var description in provider.ApiVersionDescriptions)
            {
                option.SwaggerDoc(description.GroupName, new OpenApiInfo
                {
                    Title = $"{apiTitle} {description.ApiVersion}",
                    Version = description.ApiVersion.ToString(),
                    Description = description.IsDeprecated ? "Эта версия устарела." : ""
                });
            }

            option.OperationFilter<AuthorizeCheckOperationFilter>();

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