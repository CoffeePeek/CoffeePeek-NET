namespace CoffeePeek.Gateway.Extensions;

public static class ApplicationExtensions
{
    public static void ConfigureCustomCaching(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            await next();
            if (!context.Response.HasStarted &&
                HttpMethods.IsGet(context.Request.Method) &&
                context.Request.Path.StartsWithSegments("/api/vacancies", StringComparison.OrdinalIgnoreCase) &&
                context.Response.StatusCode == StatusCodes.Status200OK)
            {
                context.Response.Headers.CacheControl = "public,max-age=60";
            }
        });
    }

    public static void ConfigureSwaggerEndpoints(this WebApplication app, ILogger logger)
    {
        var isDebug = Environment.GetEnvironmentVariable("IS_DEBUG");
        var showSwagger = app.Environment.IsDevelopment() || 
                          (!string.IsNullOrEmpty(isDebug) && 
                           (isDebug.Equals("true", StringComparison.OrdinalIgnoreCase) || 
                            isDebug.Equals("1", StringComparison.OrdinalIgnoreCase)));


        logger.LogInformation("Swagger enabled: {ShowSwagger}", showSwagger);

        if (showSwagger)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                // Убираем дублирование, используя список из кластеров/конфигурации
                var swaggerEndpoints = new List<(string Name, string Path)>
                {
                    ("Gateway API", "/swagger/v1/swagger.json"),
                    ("Auth Service", "/swagger/auth/v1/swagger.json"),
                    ("User Service", "/swagger/user/v1/swagger.json"),
                    ("Shops Service", "/swagger/shops/v1/swagger.json"),
                    ("Moderation Service", "/swagger/moderation/v1/swagger.json"),
                    ("Photo Service", "/swagger/photo/v1/swagger.json"),
                    ("Job Service", "/swagger/jobs/v1/swagger.json")
                };

                foreach (var (name, path) in swaggerEndpoints)
                {
                    c.SwaggerEndpoint(path, name);
                }
                
                c.RoutePrefix = "swagger";
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
                c.EnableDeepLinking();
                c.EnableFilter();
            });
        }
    }
}