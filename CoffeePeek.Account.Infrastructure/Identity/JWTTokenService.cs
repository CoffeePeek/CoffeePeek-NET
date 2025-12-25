using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Account.Domain.Aggregates.UserAggregate;
using CoffeePeek.Account.Domain.Entities;
using CoffeePeek.Shared.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CoffeePeek.Auth.Infrastructure.Identity;

public class JWTTokenService(IOptions<JWTOptions> options) : IJWTTokenService
{
    private readonly JWTOptions _options = options.Value;

    public string GenerateAccessToken(UserCredential user, string device, string ipAddress)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Locale, device),
            new(JwtRegisteredClaimNames.Address, ipAddress),
        };
        claims.AddRange(user.UserRoles.Select(ur => new Claim(ClaimTypes.Role, ur.Role.Name)));

        return CreateToken(claims, _options.AccessTokenLifetimeMinutes);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private string CreateToken(IEnumerable<Claim> claims, int lifetimeMinutes)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var token = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(lifetimeMinutes),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}