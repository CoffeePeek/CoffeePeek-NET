using CoffeePeek.Auth.Domain.Entities;

namespace CoffeePeek.Auth.Application.Services;

public interface IRoleManager
{
    Task<bool> RoleExistsAsync(string roleName);
    Task<Role> CreateRoleAsync(string roleName);
    Task<Role?> GetRoleAsync(string roleName);
    Task<IEnumerable<Role>> GetAllRolesAsync();
}