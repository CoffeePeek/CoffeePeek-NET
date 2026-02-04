using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Entities;
using CoffeePeek.Shops.Domain.Entities.UserFavoriteAggregate;
using Microsoft.EntityFrameworkCore;
using CheckIn = CoffeePeek.Shops.Domain.Aggregates.CheckInAggregate.CheckIn;

namespace CoffeePeek.Shops.Infrastructure.Services;

public class UserCheckInRepository(IGenericRepository<CheckIn> repository) : IUserCheckInRepository
{
    public Task<bool> Exists(Guid userId, Guid coffeeShopId, CancellationToken ct = default)
    {
        return repository.AnyAsync(x => x.UserId == userId && x.ShopId == coffeeShopId, ct);
    }

    public Task<List<Guid>> GetVisitedShopIdsAsync(Guid userId, CancellationToken ct = default)
    {
        return repository
            .QueryAsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.ShopId)
            .ToListAsync(ct);
    }
}