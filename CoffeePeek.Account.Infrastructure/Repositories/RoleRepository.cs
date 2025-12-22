using CoffeePeek.Account.Domain.Entities;
using CoffeePeek.Account.Domain.Repositories;
using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Auth.Infrastructure.Repositories;

public class RoleRepository(IGenericRepository<Role> roleRepository) : IRoleRepository
{
    public async Task<bool> RoleExistsAsync(string roleName)
    {
        return await roleRepository.AnyAsync(
            r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
    }

    public async Task CreateRoleAsync(Role role)
    {
        await roleRepository.AddAsync(role);
    }

    public async Task<Role?> GetRoleAsync(string roleName)
    {
        return await roleRepository.FirstOrDefaultAsync(r => r.Name == roleName);
    }

    public async Task<IEnumerable<Role>> GetAllRolesAsync()
    {
        return await roleRepository.GetAllAsync();
    }
}