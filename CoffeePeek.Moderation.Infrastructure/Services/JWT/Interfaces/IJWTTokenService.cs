using CoffeePeek.Moderation.Contract.Models.JWT;

namespace CoffeePeek.Moderation.Infrastructure.Services.JWT;

public interface IJWTTokenService
{
    Task<AuthResult> GenerateTokensAsync(Domain.Entities.Users.User user);
    Task<AuthResult> RefreshTokensAsync(string refreshToken);
}