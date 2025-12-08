using CoffeePeek.AuthService.Entities;
using CoffeePeek.AuthService.Models;
using CoffeePeek.Contract.Dtos.Auth;

namespace CoffeePeek.AuthService.Services;

public interface IJWTTokenService
{
    Task<AuthResult> GenerateTokensAsync(UserCredentials user);
    Task<AuthResult> RefreshTokensAsync(string refreshToken, Guid userId);
}