using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Aggregates.UserFavoriteAggregate;
using CoffeePeek.Shops.Domain.Entities.UserFavoriteAggregate;

namespace CoffeePeek.Shops.Infrastructure.Services;

public class UserFavoriteRepository(IGenericRepository<UserFavorite> repository) 
    : IUserFavoriteRepository
{
    public Task<bool> Exists(Guid userId, Guid coffeeShopId, CancellationToken ct = default)
    {
        return repository.AnyAsync(f => f.UserId == userId && f.CoffeeShopId == coffeeShopId, ct);
    }

    public Task<UserFavorite?> GetByUserAndShopAsync(Guid userId, Guid coffeeShopId, CancellationToken ct = default)
    {
        return repository.FirstOrDefaultAsync(
            f => f.UserId == userId && f.CoffeeShopId == coffeeShopId, ct);
    }
}