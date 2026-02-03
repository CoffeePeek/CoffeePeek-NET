using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace CoffeePeek.Shared.Infrastructure;

public interface IUserContext
{
    /// <summary>
/// Получает идентификатор текущего пользователя из контекста запроса, если он доступен.
/// </summary>
/// <returns>Идентификатор пользователя в виде `Guid`, или `null`, если идентификатор отсутствует.</returns>
Guid? GetUserId();
    /// <summary>
/// Получает идентификатор текущего пользователя и требует его наличия.
/// </summary>
/// <returns>Идентификатор пользователя.</returns>
/// <exception cref="System.UnauthorizedAccessException">Если идентификатор пользователя отсутствует.</exception>
Guid GetUserIdOrThrow();
    /// <summary>
/// Возвращает текущее имя пользователя, если оно доступно.
/// </summary>
/// <returns>Имя пользователя, или `null`, если имя недоступно.</returns>
string? GetUsername();
    /// <summary>
/// Получает имя пользователя из HTTP-заголовков или claims; если имя отсутствует, генерирует исключение.
/// </summary>
/// <returns>Строка с именем пользователя.</returns>
/// <exception cref="System.UnauthorizedAccessException">Если имя пользователя не найдено в заголовках и claims.</exception>
string GetUsernameOrThrow();
    /// <summary>
/// Возвращает роль текущего пользователя, если она доступна.
/// </summary>
/// <returns>Строка роли пользователя, или <c>null</c>, если роль отсутствует.</returns>
string? GetUserRole();
    bool IsAuthenticated { get; }
}

public class HeaderUserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public const string XUserId = "X-User-Id";
    public const string XUserName = "X-User-Name";
    public const string XUserRole = "X-User-Role";
    public const string XUserEmail = "X-User-Email";

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="HeaderUserContext"/> и сохраняет предоставленный <see cref="IHttpContextAccessor"/> для доступа к текущему HTTP-контексту.
    /// </summary>
    public HeaderUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(GetUserId()?.ToString());

    /// <summary>
    /// Получает идентификатор пользователя из текущего HTTP-контекста.
    /// </summary>
    /// <remarks>
    /// Предпочтительно извлекает значение из заголовка X-User-Id; при отсутствии заголовка пытается получить значение из клейма ClaimTypes.NameIdentifier.
    /// </remarks>
    /// <returns>Идентификатор пользователя как `Guid`, или `null`, если значение недоступно.</returns>
    public Guid? GetUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return null;

        // First try to get from headers (from Gateway)
        if (httpContext.Request.Headers.TryGetValue(XUserId, out var userIdHeader))
        {
            if (Guid.TryParse(userIdHeader.FirstOrDefault(), out var userId))
                return userId;
        }

        // Fallback to claims (if JWT is still present)
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var claimUserId))
            return claimUserId;

        return null;
    }

    /// <summary>
    /// Получает идентификатор пользователя или выбрасывает исключение при его отсутствии.
    /// </summary>
    /// <returns>Идентификатор пользователя.</returns>
    /// <exception cref="UnauthorizedAccessException">Бросается, если идентификатор пользователя отсутствует.</exception>
    public Guid GetUserIdOrThrow()
    {
        var userId = GetUserId();
        if (userId == null)
            throw new UnauthorizedAccessException("User ID is missing.");
        return userId.Value;
    }

    /// <summary>
    /// Получает имя пользователя, сначала пытаясь прочитать заголовок X-User-Name, затем — соответствующие claims.
    /// </summary>
    /// <returns>`null`, если HTTP-контекст отсутствует или имя пользователя не найдено; иначе значение имени пользователя.</returns>
    public string? GetUsername()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return null;

        // First try to get from headers (from Gateway)
        if (httpContext.Request.Headers.TryGetValue(XUserName, out var userNameHeader))
        {
            var username = userNameHeader.FirstOrDefault();
            if (!string.IsNullOrEmpty(username))
                return username;
        }

        // Fallback to claims
        return httpContext.User.FindFirst(ClaimTypes.Name)?.Value
               ?? httpContext.User.FindFirst("preferred_username")?.Value;
    }

    /// <summary>
    /// Получает имя пользователя из текущего HTTP-контекста или выбрасывает исключение, если оно отсутствует.
    /// </summary>
    /// <returns>Имя пользователя из контекста.</returns>
    /// <exception cref="UnauthorizedAccessException">Выбрасывается, если имя пользователя отсутствует (сообщение: "Username is missing.").</exception>
    public string GetUsernameOrThrow()
    {
        var username = GetUsername();
        if (string.IsNullOrEmpty(username))
            throw new UnauthorizedAccessException("Username is missing.");
        return username;
    }

    /// <summary>
    /// Получает роль текущего пользователя.
    /// Предпочитает значение заголовка X-User-Role; при его отсутствии возвращает значение претензии ClaimTypes.Role.
    /// </summary>
    /// <returns>Строка с ролью пользователя, или <c>null</c>, если роль не обнаружена.</returns>
    public string? GetUserRole()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return null;

        // First try to get from headers (from Gateway)
        if (httpContext.Request.Headers.TryGetValue(XUserRole, out var roleHeader))
        {
            var role = roleHeader.FirstOrDefault();
            if (!string.IsNullOrEmpty(role))
                return role;
        }

        // Fallback to claims
        return httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
    }
}