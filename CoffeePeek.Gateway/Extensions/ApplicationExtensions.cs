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
}