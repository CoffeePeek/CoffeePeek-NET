using System.Security.Claims;

namespace CoffeePeek.Gateway.Extensions;

public static class ClaimsTransformationExtensions
{
    public const string XUserId = "X-User-Id";
    public const string XUserName = "X-User-Name";
    public const string XUserRole = "X-User-Role";
    public const string XUserEmail = "X-User-Email";

    /// <summary>
    /// Формирует словарь HTTP-заголовков на основе утверждений (claims) текущего пользователя.
    /// </summary>
    /// <param name="user">Объект пользователя (ClaimsPrincipal). Если пользователь не аутентифицирован, будет возвращён пустой словарь.</param>
    /// <returns>Словарь, где ключи — имена заголовков и значения — соответствующие значения утверждений. Пустой словарь возвращается, если пользователь не аутентифицирован или соответствующие утверждения отсутствуют.</returns>
    /// <remarks>
    /// Соответствие утверждений и заголовков:
    /// - `ClaimTypes.NameIdentifier` → X-User-Id
    /// - `ClaimTypes.Name` или `preferred_username` → X-User-Name
    /// - `ClaimTypes.Email` → X-User-Email
    /// - `ClaimTypes.Role` → X-User-Role
    /// </remarks>
    public static Dictionary<string, string> ExtractClaimsAsHeaders(ClaimsPrincipal user)
    {
        var headers = new Dictionary<string, string>();

        if (!user.Identity?.IsAuthenticated ?? true)
            return headers;

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
            headers[XUserId] = userId;

        var userName = user.FindFirst(ClaimTypes.Name)?.Value
                       ?? user.FindFirst("preferred_username")?.Value;
        if (!string.IsNullOrEmpty(userName))
            headers[XUserName] = userName;

        var email = user.FindFirst(ClaimTypes.Email)?.Value;
        if (!string.IsNullOrEmpty(email))
            headers[XUserEmail] = email;

        var role = user.FindFirst(ClaimTypes.Role)?.Value;
        if (!string.IsNullOrEmpty(role))
            headers[XUserRole] = role;

        return headers;
    }
}