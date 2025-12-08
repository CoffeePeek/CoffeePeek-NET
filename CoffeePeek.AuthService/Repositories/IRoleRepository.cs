using CoffeePeek.AuthService.Entities;
using CoffeePeek.AuthService.Models;

namespace CoffeePeek.AuthService.Repositories;

public interface IRoleRepository
{
    Task<bool> RoleExistsAsync(string roleName);
    Task CreateRoleAsync(Role role);
    Task<Role?> GetRoleAsync(string roleName);
    Task<IEnumerable<Role>> GetAllRolesAsync();
}