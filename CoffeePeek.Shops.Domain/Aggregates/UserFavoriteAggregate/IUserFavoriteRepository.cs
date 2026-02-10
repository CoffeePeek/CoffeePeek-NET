using CoffeePeek.Shops.Domain.Entities.UserFavoriteAggregate;

namespace CoffeePeek.Shops.Domain.Aggregates.UserFavoriteAggregate;

public interface IUserFavoriteRepository
{
    Task<bool> Exists(Guid userId, Guid coffeeShopId, CancellationToken ct = default);
    Task<UserFavorite?> GetByUserAndShopAsync(Guid userId, Guid coffeeShopId, CancellationToken ct = default);
}