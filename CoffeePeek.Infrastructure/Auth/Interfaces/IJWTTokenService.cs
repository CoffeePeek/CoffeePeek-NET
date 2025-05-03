using CoffeePeek.Contract.Dtos.Auth;
using CoffeePeek.Domain.Entities.Users;

namespace CoffeePeek.Infrastructure.Auth;

public interface IJWTTokenService
{
    Task<AuthResult> GenerateTokensAsync(User user);
    Task<AuthResult> RefreshTokensAsync(string refreshToken);
}