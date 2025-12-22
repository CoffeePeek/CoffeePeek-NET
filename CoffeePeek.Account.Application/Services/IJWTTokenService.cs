using CoffeePeek.Account.Domain.Entities;
using CoffeePeek.Contract.Dtos.Auth;

namespace CoffeePeek.Account.Application.Services;

public interface IJWTTokenService
{
    Task<AuthResult> GenerateTokensAsync(UserCredential user, string deviceName, string ipAddress);
    Task<AuthResult> RefreshTokensAsync(string refreshToken, Guid userId, string deviceName, string ipAddress);
}