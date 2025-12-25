using CoffeePeek.Account.Domain.Entities;

namespace CoffeePeek.Account.Domain.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetRoleAsync(string roleName);
}