using CoffeePeek.Account.Domain.Entities;

namespace CoffeePeek.Account.Application.Services;

public interface IRoleManager
{
    Task<bool> RoleExistsAsync(string roleName);
    Task<Role> CreateRoleAsync(string roleName);
    Task<Role?> GetRoleAsync(string roleName);
    Task<IEnumerable<Role>> GetAllRolesAsync();
}