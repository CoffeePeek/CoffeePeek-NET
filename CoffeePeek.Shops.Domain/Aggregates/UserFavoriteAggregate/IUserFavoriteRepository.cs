namespace CoffeePeek.Shops.Domain.Aggregates.UserFavoriteAggregate;

public interface IUserFavoriteRepository
{
    void Remove(UserFavorite favorite);
    void Add(UserFavorite favorite);
    
    Task<bool> Exists(Guid userId, Guid coffeeShopId, CancellationToken ct = default);
    Task<UserFavorite?> GetByUserAndShopAsync(Guid userId, Guid coffeeShopId, CancellationToken ct = default);
}