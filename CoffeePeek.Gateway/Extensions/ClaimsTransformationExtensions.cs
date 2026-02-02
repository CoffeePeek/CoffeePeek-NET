using System.Security.Claims;

namespace CoffeePeek.Gateway.Extensions;

public static class ClaimsTransformationExtensions
{
    public const string XUserId = "X-User-Id";
    public const string XUserName = "X-User-Name";
    public const string XUserRole = "X-User-Role";
    public const string XUserEmail = "X-User-Email";

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
