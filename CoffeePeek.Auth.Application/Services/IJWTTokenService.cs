using CoffeePeek.Auth.Domain.Entities;
using CoffeePeek.Contract.Dtos.Auth;

namespace CoffeePeek.Auth.Application.Services;

public interface IJWTTokenService
{
    Task<AuthResult> GenerateTokensAsync(UserCredentials user, string deviceName, string ipAddress);
    Task<AuthResult> RefreshTokensAsync(string refreshToken, Guid userId, string deviceName, string ipAddress);
}