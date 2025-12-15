using CoffeePeek.AuthService.Entities;
using CoffeePeek.Contract.Dtos.Auth;

namespace CoffeePeek.AuthService.Services;

public interface IJWTTokenService
{
    Task<AuthResult> GenerateTokensAsync(UserCredentials user, string deviceName, string ipAddress);
    Task<AuthResult> RefreshTokensAsync(string refreshToken, Guid userId, string deviceName, string ipAddress);
}