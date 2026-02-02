using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace CoffeePeek.Shared.Infrastructure;

public interface IUserContext
{
    Guid? GetUserId();
    Guid GetUserIdOrThrow();
    string? GetUsername();
    string GetUsernameOrThrow();
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

    public HeaderUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(GetUserId()?.ToString());

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

    public Guid GetUserIdOrThrow()
    {
        var userId = GetUserId();
        if (userId == null)
            throw new UnauthorizedAccessException("User ID is missing.");
        return userId.Value;
    }

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

    public string GetUsernameOrThrow()
    {
        var username = GetUsername();
        if (string.IsNullOrEmpty(username))
            throw new UnauthorizedAccessException("Username is missing.");
        return username;
    }

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
