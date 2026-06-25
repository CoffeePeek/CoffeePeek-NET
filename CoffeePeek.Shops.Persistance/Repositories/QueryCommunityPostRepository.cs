using CoffeePeek.Shops.Domain.Aggregates.CommunityPostAggregate;
using CoffeePeek.Shops.Persistance.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Persistance.Repositories;

public class QueryCommunityPostRepository(ShopsDbContext dbContext) : IQueryCommunityPostRepository
{
    public Task<bool> ExistsByIdAsync(Guid postId, CancellationToken ct = default) =>
        dbContext.CommunityPosts
            .AsNoTracking()
            .AnyAsync(p => p.Id == postId && !p.IsSoftDelete, ct);

    public Task<CommunityPost?> GetByIdAsync(Guid postId, CancellationToken ct = default) =>
        dbContext.CommunityPosts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == postId && !p.IsSoftDelete, ct);
}
