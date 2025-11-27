namespace CoffeePeek.Web.Middleware;

public class AuthTokenMiddleware(RequestDelegate next, ILogger<AuthTokenMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        if (path.StartsWith("/css") || path.StartsWith("/js") || path.StartsWith("/lib") ||
            path.StartsWith("/login") || path.StartsWith("/register") ||
            path == "/" || path.StartsWith("/error") || path.StartsWith("/index"))
        {
            await next(context);
            return;
        }

        var accessToken = context.Session.GetString("AccessToken") ??
                          context.Request.Cookies["AccessToken"];

        if (!string.IsNullOrEmpty(accessToken))
        {
            context.Items["AccessToken"] = accessToken;
            logger.LogDebug("Token found for path: {Path}", path);
        }
        else
        {
            if (path.StartsWith("/moderation"))
            {
                logger.LogWarning("Access to /moderation denied - no token");
                context.Response.Redirect("/Login");
                return;
            }

            logger.LogDebug("No token for path: {Path}, allowing access", path);
        }

        await next(context);
    }
}