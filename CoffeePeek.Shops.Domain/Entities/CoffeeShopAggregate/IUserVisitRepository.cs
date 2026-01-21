namespace CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;

/// <summary>
/// Расширенный репозиторий для работы с посещёнными местами
/// </summary>
public interface IUserVisitRepository
{
    Task<bool> ExistsAsync(Guid userId, Guid shopId, CancellationToken ct = default);
    Task<UserVisit?> GetByUserAndShopAsync(Guid userId, Guid shopId, CancellationToken ct = default);
    Task<List<Guid>> GetVisitedShopIdsAsync(Guid userId, CancellationToken ct = default);
    Task<int> GetVisitedCountAsync(Guid userId, CancellationToken ct = default);
    Task<int> GetTotalVisitsCountAsync(Guid userId, CancellationToken ct = default);
}
