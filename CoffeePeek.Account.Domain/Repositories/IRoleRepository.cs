using CoffeePeek.Account.Domain.Aggregates.UserAggregate;

namespace CoffeePeek.Account.Domain.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetRoleAsync(string roleName);
}