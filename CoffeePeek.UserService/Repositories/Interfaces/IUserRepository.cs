using CoffeePeek.UserService.Models;

namespace CoffeePeek.UserService.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid eventUserId);
    Task AddAsync(User user);
}