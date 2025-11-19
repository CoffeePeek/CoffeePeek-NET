using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CoffeePeek.Web.Middleware;

public class AuthTokenMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthTokenMiddleware> _logger;

    public AuthTokenMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<AuthTokenMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Пропускаем статические файлы и страницы авторизации
        var path = context.Request.Path.Value?.ToLower() ?? "";
        if (path.StartsWith("/css") || path.StartsWith("/js") || path.StartsWith("/lib") || 
            path.StartsWith("/login") || path.StartsWith("/register") || 
            path == "/" || path.StartsWith("/error") || path.StartsWith("/index"))
        {
            await _next(context);
            return;
        }

        // Проверяем наличие токена в сессии или cookies
        var accessToken = context.Session.GetString("AccessToken") ?? 
                         context.Request.Cookies["AccessToken"];

        if (!string.IsNullOrEmpty(accessToken))
        {
            // Добавляем токен в заголовки для последующих запросов к API
            context.Items["AccessToken"] = accessToken;
            _logger.LogDebug("Token found for path: {Path}", path);
        }
        else
        {
            // Нет токена - для защищенных страниц редирект на логин
            if (path.StartsWith("/moderation"))
            {
                _logger.LogWarning("Access to /moderation denied - no token");
                context.Response.Redirect("/Login");
                return;
            }
            _logger.LogDebug("No token for path: {Path}, allowing access", path);
        }

        await _next(context);
    }
}
