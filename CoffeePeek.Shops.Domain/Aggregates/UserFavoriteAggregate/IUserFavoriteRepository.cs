namespace CoffeePeek.Shops.Domain.Aggregates.UserFavoriteAggregate;

public interface IUserFavoriteRepository
{
    Task<bool> Exists(Guid userId, Guid coffeeShopId, CancellationToken ct = default);
    Task<UserFavorite?> GetByUserAndShopAsync(Guid userId, Guid coffeeShopId, CancellationToken ct = default);
    Task<List<Guid>> GetFavoriteShopIdsAsync(Guid userId, CancellationToken ct = default);
    Task<int> GetFavoritesCountAsync(Guid userId, CancellationToken ct = default);
}