using CoffeePeek.Domain.Entities.Users;

namespace CoffeePeek.Infrastructure.Services.Auth.Interfaces;

public interface IAuthService
{
    Task<string> GenerateToken(User user);
    string GenerateRefreshToken(int userId);
    int? DecryptRefreshToken(string refreshToken);
}