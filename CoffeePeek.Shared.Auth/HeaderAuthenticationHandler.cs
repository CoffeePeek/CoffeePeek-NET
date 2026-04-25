using System.Security.Claims;
using System.Text.Encodings.Web;
using CoffeePeek.Shared.Auth.Constants;
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
        var roles = Context.Request.Headers[GatewayHeaderConsts.XUserRole].ToString()
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var userId = Context.Request.Headers[GatewayHeaderConsts.XUserId].ToString();

        if (string.IsNullOrEmpty(userId))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        AddClaimFromHeader(claims, GatewayHeaderConsts.XUserName, ClaimTypes.Name);
        AddClaimFromHeader(claims, GatewayHeaderConsts.XUserEmail, ClaimTypes.Email);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private void AddClaimFromHeader(List<Claim> claims, string headerName, string claimType)
    {
        var value = Context.Request.Headers[headerName].ToString();
        if (!string.IsNullOrWhiteSpace(value))
        {
            claims.Add(new Claim(claimType, value));
        }
    }
}
