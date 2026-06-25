using CoffeePeek.Moderation.Domain.Aggregates.ModerationCommunityPostAggregate;
using CoffeeShop.Moderation.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Moderation.Persistence.Repositories;

public class ModerationCommunityPostRepository(ModerationDbContext dbContext) : IModerationCommunityPostRepository
{
    public void Add(ModerationCommunityPost post) => dbContext.ModerationCommunityPosts.Add(post);

    public Task<ModerationCommunityPost?> GetById(Guid id, CancellationToken ct = default) =>
        dbContext.ModerationCommunityPosts.FirstOrDefaultAsync(p => p.Id == id, ct);
}
