using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Entities.UserFavoriteAggregate;
using Microsoft.EntityFrameworkCore;

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

    public async Task<List<Guid>> GetFavoriteShopIdsAsync(Guid userId, CancellationToken ct = default)
    {
        return await repository
            .Query()
            .Where(f => f.UserId == userId)
            .OrderBy(f => f.CreatedAtUtc)
            .Select(f => f.CoffeeShopId)
            .ToListAsync(ct);
    }

    public Task<int> GetFavoritesCountAsync(Guid userId, CancellationToken ct = default)
    {
        return repository.CountAsync(f => f.UserId == userId, ct);
    }
}