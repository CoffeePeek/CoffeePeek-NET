using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace CoffeePeek.Shared.Infrastructure;

public class HeaderAuthMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IUserContext userContext)
    {
        if (userContext.IsAuthenticated)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userContext.GetUserId().ToString()!),
                new Claim(ClaimTypes.Name, userContext.GetUsername() ?? "Unknown")
            };

            var rolesHeader = context.Request.Headers[HeaderUserContext.XUserRole].ToString();
            if (!string.IsNullOrEmpty(rolesHeader))
            {
                var roles = rolesHeader.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Trim()));
                }
            }

            var identity = new ClaimsIdentity(claims, "HeaderAuth");
            context.User = new ClaimsPrincipal(identity);
        }

        await next(context);
    }
}