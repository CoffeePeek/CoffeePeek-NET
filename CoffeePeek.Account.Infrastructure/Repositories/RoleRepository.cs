using CoffeePeek.Account.Domain.Entities.RoleAggregate;
using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Auth.Infrastructure.Repositories;

public class RoleRepository(IGenericRepository<Role> roleRepository) : IRoleRepository
{
    public async Task<Role?> GetRoleAsync(string roleName)
    {
        return await roleRepository.FirstOrDefaultAsync(r => r.Name == roleName);
    }
}