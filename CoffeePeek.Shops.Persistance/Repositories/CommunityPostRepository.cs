using CoffeePeek.Shops.Domain.Aggregates.CommunityPostAggregate;
using CoffeePeek.Shops.Persistance.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Persistance.Repositories;

public class CommunityPostRepository(ShopsDbContext dbContext) : ICommunityPostRepository
{
    public void Add(CommunityPost post) => dbContext.CommunityPosts.Add(post);

    public Task<bool> ExistsByModerationPostIdAsync(Guid moderationPostId, CancellationToken ct = default) =>
        dbContext.CommunityPosts.AsNoTracking().AnyAsync(p => p.ModerationPostId == moderationPostId, ct);
}
