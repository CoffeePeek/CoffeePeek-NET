using CoffeePeek.Domain.Entities.Auth;

namespace CoffeePeek.Infrastructure.Services.User.Interfaces;

public interface IUserManager
{
    bool CheckPasswordAsync(Domain.Entities.Users.User user, string requestPassword);
    Task<Domain.Entities.Users.User?> FindByEmailAsync(string requestEmail);
    Task CreateAsync(Domain.Entities.Users.User user, string requestPassword);
    Task<ICollection<Role>> GetRolesAsync(Domain.Entities.Users.User user);
}