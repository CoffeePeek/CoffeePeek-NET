namespace CoffeePeek.Infrastructure.Services.Auth.Interfaces;

[Obsolete]
public interface IAuthService
{
    Task<string> GenerateToken(Domain.Entities.Users.User user);
    string GenerateRefreshToken(int userId);
    int? DecryptRefreshToken(string refreshToken);
}