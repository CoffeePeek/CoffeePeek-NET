using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CoffeePeek.Shared.Extensions.Swagger;

public static class SwaggerPipelineExtensions
{
    private static bool IsDebugMode()
    {
        var isDebug = Environment.GetEnvironmentVariable("IS_DEBUG");
        return !string.IsNullOrEmpty(isDebug) && 
               (isDebug.Equals("true", StringComparison.OrdinalIgnoreCase) || 
                isDebug.Equals("1", StringComparison.OrdinalIgnoreCase));
    }

    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        if (app.Environment.IsDevelopment() || IsDebugMode())
        {
            app.UseSwagger();
        
            var swaggerGenOptions = app.Services.GetRequiredService<IOptions<SwaggerGenOptions>>().Value;
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

    public static WebApplication UseSwaggerDocumentation(this WebApplication app, string version)
    {
        if (app.Environment.IsDevelopment() || IsDebugMode())
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

