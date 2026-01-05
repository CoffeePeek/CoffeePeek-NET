using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Account.Domain.Aggregates.UserAggregate;
using CoffeePeek.Account.Domain.Services;
using CoffeePeek.Contract.Dtos.Auth;
using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CoffeePeek.Account.Application.Features.Auth.Login;

public class AuthService(
    IJWTTokenService jwtTokenService,
    IGenericRepository<UserCredential> userCredentialRepository,
    IPasswordHasherService passwordHasher,
    IOptions<JWTOptions> jwtOptions) : IAuthService
{
    public async Task<AuthResult> LoginAsync(string email, string password, string device, string ip)
    {
        var userCredential = await userCredentialRepository
            .QueryAsNoTracking()
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Email == email);
        
        if (userCredential == null || !userCredential.ValidatePassword(password, passwordHasher))
        {
            throw new UnauthorizedException("Invalid credentials");
        }

        userCredential.RevokeAllSessions();
        var accessToken = jwtTokenService.GenerateAccessToken(userCredential, device, ip);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        var authResult = new AuthResult {AccessToken = accessToken, RefreshToken = refreshToken};
        
        userCredential.AddRefreshToken(authResult.AccessToken,
            TimeSpan.FromMinutes(jwtOptions.Value.AccessTokenLifetimeMinutes), device, ip);

        return authResult;
    }
}