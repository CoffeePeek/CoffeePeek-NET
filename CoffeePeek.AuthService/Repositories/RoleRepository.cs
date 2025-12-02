using CoffeePeek.AuthService.Configuration;
using CoffeePeek.AuthService.Entities;
using CoffeePeek.AuthService.Models;

namespace CoffeePeek.AuthService.Repositories;

public class RoleRepository(AuthDbContext context) : IRoleRepository
{
    public Task<bool> RoleExistsAsync(string roleName) =>
        Task.FromResult(context.Roles.Any(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase)));

    public Task CreateRoleAsync(Role role)
    {
        context.Roles.Add(role);
        return Task.CompletedTask;
    }

    public Task<Role?> GetRoleAsync(string roleName) =>
        Task.FromResult(context.Roles.FirstOrDefault(r => r.Name == roleName));

    public Task<IEnumerable<Role>> GetAllRolesAsync() => Task.FromResult(context.Roles.AsEnumerable());
}