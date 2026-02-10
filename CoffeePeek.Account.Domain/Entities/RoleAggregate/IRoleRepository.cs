namespace CoffeePeek.Account.Domain.Entities.RoleAggregate;

public interface IRoleRepository
{
    Task<Role?> GetRoleAsync(string roleName);
    Task<Role?> GetByIdAsync(Guid requestRoleId, CancellationToken ct);
}