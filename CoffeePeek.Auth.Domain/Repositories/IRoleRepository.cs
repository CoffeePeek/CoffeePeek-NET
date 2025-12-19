using CoffeePeek.Auth.Domain.Entities;

namespace CoffeePeek.Auth.Domain.Repositories;

public interface IRoleRepository
{
    Task<bool> RoleExistsAsync(string roleName);
    Task CreateRoleAsync(Role role);
    Task<Role?> GetRoleAsync(string roleName);
    Task<IEnumerable<Role>> GetAllRolesAsync();
}