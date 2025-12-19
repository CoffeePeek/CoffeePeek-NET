using System.Security.Claims;
using CoffeePeek.Auth.Domain.Entities;
using CoffeePeek.AuthService.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace CoffeePeek.Auth.Application.Services;

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