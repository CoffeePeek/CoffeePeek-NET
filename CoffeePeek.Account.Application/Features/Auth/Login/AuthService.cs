using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Account.Domain.Services;
using CoffeePeek.Contract.Dtos.Auth;
using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace CoffeePeek.Account.Application.Features.Auth.Login;

public class AuthService(
    IJWTTokenService jwtTokenService,
    IUserRepository userRepository,
    IPasswordHasherService passwordHasher,
    IOptions<JWTOptions> jwtOptions) : IAuthService
{
    public async Task<AuthResult> LoginAsync(string email, string password, string device, string ip)
    {
        var user = await userRepository.GetByEmail(email, CancellationToken.None);

        if (user == null)
        {
            throw new NotFoundException("User not found");
        }
        
        if (!user.Credentials.ValidatePassword(password, passwordHasher))
        {
            throw new UnauthorizedException("Invalid credentials");
        }

        user.RevokeAllSessions();
        var accessToken = jwtTokenService.GenerateAccessToken(user, device, ip);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        var authResult = new AuthResult {AccessToken = accessToken, RefreshToken = refreshToken};
        
        user.AddSession(authResult.AccessToken,
            ttl:TimeSpan.FromMinutes(jwtOptions.Value.AccessTokenLifetimeMinutes), device, ip);

        return authResult;
    }
}