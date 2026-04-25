using CoffeePeek.Shops.Domain.Aggregates.UserFavoriteAggregate;
using CoffeePeek.Shops.Persistance.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Persistance.Repositories;

public class UserFavoriteRepository(ShopsDbContext dbContext) : IUserFavoriteRepository
{
    private readonly DbSet<UserFavorite> _repository = dbContext.UserFavorites;

    public void Remove(UserFavorite favorite)
    {
        dbContext.Remove(favorite);
    }

    public void Add(UserFavorite favorite)
    {
        dbContext.Add(favorite);
    }

    public Task<bool> Exists(Guid userId, Guid coffeeShopId, CancellationToken ct = default)
    {
        return _repository.AnyAsync(f => f.UserId == userId && f.CoffeeShopId == coffeeShopId, ct);
    }

    public Task<UserFavorite?> GetByUserAndShopAsync(Guid userId, Guid coffeeShopId, CancellationToken ct = default)
    {
        return _repository
            .FirstOrDefaultAsync(uf => uf.UserId == userId && uf.CoffeeShopId == coffeeShopId, ct);
    }

    public async Task<List<Guid>> GetFavoriteShopIdsAsync(Guid userId, CancellationToken ct = default)
    {
        return await _repository
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .OrderBy(f => f.CreatedAtUtc)
            .Select(f => f.CoffeeShopId)
            .ToListAsync(ct);
    }

    public Task<int> GetFavoritesCountAsync(Guid userId, CancellationToken ct = default)
    {
        return _repository.CountAsync(f => f.UserId == userId, ct);
    }
}