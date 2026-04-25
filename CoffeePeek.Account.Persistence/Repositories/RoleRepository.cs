using CoffeePeek.Account.Domain.Entities.RoleAggregate;
using CoffeePeek.Account.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Account.Persistence.Repositories;

public class RoleRepository(AccountDbContext accountDbContext) : IRoleRepository
{
    private readonly DbSet<Role> _repository = accountDbContext.Roles;
    
    public async Task<Role?> GetRoleAsync(string roleName)
    {
        return await _repository.FirstOrDefaultAsync(r => r.Name == roleName);
    }

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _repository.FirstOrDefaultAsync(r => r.Id == id, ct);
    }
}