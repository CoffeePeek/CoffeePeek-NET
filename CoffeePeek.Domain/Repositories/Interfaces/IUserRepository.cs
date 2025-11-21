using CoffeePeek.Domain.Entities.Auth;
using CoffeePeek.Domain.Entities.Users;
using CoffeePeek.Domain.UnitOfWork;

namespace CoffeePeek.Domain.Repositories.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetUserByRefreshToken(string refreshToken);
    Task<ICollection<Role>> GetUserRoles(User user);
}