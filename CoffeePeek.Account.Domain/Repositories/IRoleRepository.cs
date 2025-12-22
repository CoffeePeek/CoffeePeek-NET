using CoffeePeek.Account.Domain.Entities;

namespace CoffeePeek.Account.Domain.Repositories;

public interface IRoleRepository
{
    Task<bool> RoleExistsAsync(string roleName);
    Task CreateRoleAsync(Role role);
    Task<Role?> GetRoleAsync(string roleName);
    Task<IEnumerable<Role>> GetAllRolesAsync();
}