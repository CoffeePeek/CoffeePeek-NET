using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

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
    
    extension(WebApplication app)
    {
        public WebApplication UseSwaggerDocumentation()
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
            
                var swaggerGenOptions = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<SwaggerGenOptions>>().Value;
                var documentNames = swaggerGenOptions.SwaggerGeneratorOptions.SwaggerDocs.Keys.ToList();
            
                app.UseSwaggerUI(c =>
                {
                    foreach (var docName in documentNames)
                    {
                        c.SwaggerEndpoint($"/swagger/{docName}/swagger.json", docName);
                    }
                
                    c.RoutePrefix = "swagger";
                    c.DisplayRequestDuration();
                    c.EnableDeepLinking();
                    c.EnableFilter();
                    c.ShowExtensions();
                    c.EnableValidator();
                });
            }

            return app;
        }

        public WebApplication UseSwaggerDocumentation(string version)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger(c =>
                {
                    c.RouteTemplate = "swagger/{documentName}/swagger.json";
                });
            
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"API {version}");
                    c.RoutePrefix = "swagger";
                    c.DisplayRequestDuration();
                    c.EnableDeepLinking();
                    c.EnableFilter();
                    c.ShowExtensions();
                    c.EnableValidator();
                });
            }

            return app;
        }
    }
}