namespace CoffeePeek.Shops.Domain.Entities.UserFavoriteAggregate;

public interface IUserCheckInRepository
{
    Task<bool> Exists(Guid userId, Guid coffeeShopId, CancellationToken ct = default);
    Task<List<Guid>> GetVisitedShopIdsAsync(Guid userId, CancellationToken ct = default);
}