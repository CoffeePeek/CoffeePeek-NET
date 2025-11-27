using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CoffeePeek.BuildingBlocks.AuthOptions;
using CoffeePeek.Contract.Dtos.Auth;
using CoffeePeek.Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CoffeePeek.Infrastructure.Auth;

public class JWTTokenService(UserManager<User> userManager, IOptions<JWTOptions> options) : IJWTTokenService
{
    private readonly JWTOptions _options = options.Value;

    public async Task<AuthResult> GenerateTokensAsync(User user)
    {
        var accessToken = await GenerateJwtAsync(user);

        var refreshToken = await userManager.GenerateUserTokenAsync(
            user,
            TokenOptions.DefaultProvider,
            "RefreshToken"
        );

        await userManager.SetAuthenticationTokenAsync(
            user,
            TokenOptions.DefaultProvider,
            "RefreshToken",
            refreshToken
        );

        return new AuthResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<AuthResult> RefreshTokensAsync(string refreshToken, int userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new UnauthorizedAccessException("Invalid user.");

        var storedToken = await userManager.GetAuthenticationTokenAsync(
            user,
            TokenOptions.DefaultProvider,
            "RefreshToken"
        );

        if (storedToken != refreshToken)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        await userManager.RemoveAuthenticationTokenAsync(
            user,
            TokenOptions.DefaultProvider,
            "RefreshToken"
        );

        return await GenerateTokensAsync(user);
    }

    private async Task<string> GenerateJwtAsync(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Nickname, user.UserName)
        };
        
        var roles = await userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.AccessTokenLifetimeMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}