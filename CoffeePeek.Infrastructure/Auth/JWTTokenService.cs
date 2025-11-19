using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CoffeePeek.BuildingBlocks.AuthOptions;
using CoffeePeek.Contract.Dtos.Auth;
using CoffeePeek.Domain.Entities.Auth;
using CoffeePeek.Domain.Entities.Users;
using CoffeePeek.Domain.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CoffeePeek.Infrastructure.Auth;

public class JWTTokenService(IOptions<JWTOptions> options, UserRepository userRepository) : IJWTTokenService
{
    private readonly JWTOptions _options = options.Value;

    public async Task<AuthResult> GenerateTokensAsync(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_options.SecretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            ]),
            Expires = DateTime.UtcNow.AddMinutes(_options.AccessTokenLifetimeMinutes),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };

        var accessToken = tokenHandler.CreateToken(tokenDescriptor);
        var accessTokenString = tokenHandler.WriteToken(accessToken);

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = Guid.NewGuid().ToString(),
            ExpiryDate = DateTime.UtcNow.AddDays(_options.RefreshTokenLifetimeDays)
        };

        user.RefreshTokens.Add(refreshToken);
        userRepository.Update(user);
        await userRepository.SaveChangesAsync(CancellationToken.None);

        return new AuthResult
        {
            AccessToken = accessTokenString,
            RefreshToken = refreshToken.Token,
            ExpiredAt = tokenDescriptor.Expires!.Value
        };
    }

    public async Task<AuthResult> RefreshTokensAsync(string refreshToken)
    {
        var user = await userRepository.GetUserByRefreshToken(refreshToken);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        var token = user.RefreshTokens.FirstOrDefault(rt => rt.Token == refreshToken && rt.IsActive);
        if (token == null)
        {
            throw new UnauthorizedAccessException("Refresh token is expired or revoked.");
        }

        token.IsRevoked = true;
        return await GenerateTokensAsync(user);
    }
}