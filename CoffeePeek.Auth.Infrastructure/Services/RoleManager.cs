using CoffeePeek.Auth.Domain.Entities;
using CoffeePeek.Auth.Domain.Repositories;

namespace CoffeePeek.Auth.Application.Services;

public class RoleManager(IRoleRepository roleRepository) : IRoleManager
{
    public async Task<bool> RoleExistsAsync(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            throw new ArgumentException("Role name cannot be empty.", nameof(roleName));

        return await roleRepository.RoleExistsAsync(roleName);
    }

    public async Task<Role> CreateRoleAsync(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            throw new ArgumentException("Role name cannot be empty.", nameof(roleName));

        if (await RoleExistsAsync(roleName))
            throw new InvalidOperationException($"Role '{roleName}' already exists.");

        var role = new Role { Name = roleName };
        await roleRepository.CreateRoleAsync(role);
        return role;
    }

    public async Task<Role?> GetRoleAsync(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            throw new ArgumentException("Role name cannot be empty.", nameof(roleName));

        return await roleRepository.GetRoleAsync(roleName);
    }

    public async Task<IEnumerable<Role>> GetAllRolesAsync()
    {
        return await roleRepository.GetAllRolesAsync();
    }
}