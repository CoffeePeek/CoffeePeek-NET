using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CoffeePeek.AuthService.Configuration;
using CoffeePeek.AuthService.Entities;
using CoffeePeek.AuthService.Models;
using CoffeePeek.Contract.Dtos.Auth;
using CoffeePeek.Shared.Infrastructure;
using CoffeePeek.Shared.Infrastructure.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CoffeePeek.AuthService.Services;

public class JWTTokenService(IOptions<JWTOptions> options, IUserManager userManager) : IJWTTokenService
{
    private const string RefreshTokenName = "RefreshToken";

    private readonly JWTOptions _options = options.Value;

    public async Task<AuthResult> GenerateTokensAsync(UserCredentials user)
    {
        var accessToken = GenerateJwtAsync(user);

        var refreshToken = GenerateRefreshToken();

        await userManager.SetAuthenticationTokenAsync(
            user,
            TokenOptions.DefaultProvider,
            RefreshTokenName,
            refreshToken
        );

        return new AuthResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<AuthResult> RefreshTokensAsync(string refreshToken, Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid user.");

        var storedToken = userManager.GetAuthenticationTokenAsync(
            user,
            TokenOptions.DefaultProvider,
            RefreshTokenName
        );

        if (storedToken == null || storedToken != refreshToken)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        await userManager.RemoveAuthenticationTokenAsync(
            user,
            TokenOptions.DefaultProvider,
            RefreshTokenName
        );

        return await GenerateTokensAsync(user);
    }

    private string GenerateJwtAsync(UserCredentials user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
        };

        claims.AddRange(user.UserRoles.Select(role => new Claim(ClaimTypes.Role, role.Role.Name)));

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.AccessTokenLifetimeMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}