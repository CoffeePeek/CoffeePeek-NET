using CoffeePeek.Domain.Entities.Users;

namespace CoffeePeek.Infrastructure.Services;

public interface IUserService
{
    Task<User> GetOrCreateGoogleUserAsync(string googleId, string email, string name, string avatarUrl);
}