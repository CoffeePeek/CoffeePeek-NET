using CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;

namespace CoffeePeek.Shops.Domain.Entities.UserFavoriteAggregate;

public interface IUserFavoriteService
{
    Task<Result<Guid>> AddToFavoritesAsync(Guid userId, Guid coffeeShopId, CancellationToken ct = default);

    Task<Result> RemoveFromFavoritesAsync(Guid userId, Guid coffeeShopId, CancellationToken ct = default);

    Task<bool> IsFavoriteAsync(Guid userId, Guid coffeeShopId, CancellationToken ct = default);

    Task<List<CoffeeShop>> GetUserFavoritesAsync(Guid userId, CancellationToken ct = default);

    Task<int> GetFavoritesCountAsync(Guid userId, CancellationToken ct = default);
}