using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Infrastructure.Services;

public class UserVisitRepository(IGenericRepository<UserVisit> repository) 
    : IUserVisitRepository
{
    public Task<bool> ExistsAsync(Guid userId, Guid shopId, CancellationToken ct = default)
    {
        return repository.AnyAsync(v => v.UserId == userId && v.ShopId == shopId, ct);
    }

    public Task<UserVisit?> GetByUserAndShopAsync(Guid userId, Guid shopId, CancellationToken ct = default)
    {
        return repository.FirstOrDefaultAsync(
            v => v.UserId == userId && v.ShopId == shopId, ct);
    }

    public async Task<List<Guid>> GetVisitedShopIdsAsync(Guid userId, CancellationToken ct = default)
    {
        return await repository
            .Query()
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.LastVisitedAt)
            .Select(v => v.ShopId)
            .ToListAsync(ct);
    }

    public Task<int> GetVisitedCountAsync(Guid userId, CancellationToken ct = default)
    {
        return repository.CountAsync(v => v.UserId == userId, ct);
    }

    public async Task<int> GetTotalVisitsCountAsync(Guid userId, CancellationToken ct = default)
    {
        return await repository
            .Query()
            .Where(v => v.UserId == userId)
            .SumAsync(v => v.VisitCount, ct);
    }
}
