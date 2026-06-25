namespace CoffeePeek.Shops.Application.Abstractions;

public interface IUserExistenceLookup
{
    Task<bool> ExistsAsync(Guid userId, CancellationToken ct = default);
}
