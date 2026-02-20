using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoffeePeek.Shared.Auth;

public class HeaderAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var roles = Context.Request.Headers["X-User-Roles"].ToString().Split(',', StringSplitOptions.RemoveEmptyEntries);
        var userId = Context.Request.Headers["X-User-Id"].ToString();

        if (string.IsNullOrEmpty(userId))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}