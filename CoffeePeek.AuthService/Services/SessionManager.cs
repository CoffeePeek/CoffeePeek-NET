using System.Security.Claims;
using CoffeePeek.AuthService.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace CoffeePeek.AuthService.Services;

public class SessionManager(IHttpContextAccessor httpContextAccessor) : ISessionManager
{
    public async Task SignInAsync(UserCredentials user, bool isPersistent = false)
    {
        var context = httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new InvalidOperationException("HttpContext is not available.");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email), 
        };

        var claimsIdentity = new ClaimsIdentity(
            claims, 
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        var properties = new AuthenticationProperties
        {
            IsPersistent = isPersistent
        };

        await context.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            properties
        );
    }
}