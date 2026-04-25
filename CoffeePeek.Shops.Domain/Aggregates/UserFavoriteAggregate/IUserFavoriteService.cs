namespace CoffeePeek.Shops.Domain.Aggregates.UserFavoriteAggregate;

public interface IUserFavoriteService
{
    Task<Guid> AddToFavoritesAsync(Guid userId, Guid coffeeShopId, CancellationToken ct = default);

    Task RemoveFromFavoritesAsync(Guid userId, Guid coffeeShopId, CancellationToken ct = default);
}