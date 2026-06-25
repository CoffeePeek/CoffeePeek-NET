namespace CoffeePeek.Shops.Domain.Aggregates.CheckInAggregate;

public interface IQueryCheckInRepository
{
    void Add(CheckIn checkIn);
    
    Task<List<Guid>> GetVisitedShopIdsAsync(Guid userId, CancellationToken ct = default);
    Task<int> GetCheckInCountByCoffeeShopIdAsync(Guid coffeeShopId, CancellationToken ct = default);

    Task<Dictionary<Guid, int>> GetCheckInCountsByShopIdsAsync(IEnumerable<Guid> shopIds,
        CancellationToken ct = default);
    Task<bool> Exists(Guid userId, Guid shopDtoId, CancellationToken cancellationToken);
    Task<bool> ExistsByIdAsync(Guid checkInId, CancellationToken ct = default);
    Task<Guid?> GetUserIdByIdAsync(Guid checkInId, CancellationToken ct = default);
}