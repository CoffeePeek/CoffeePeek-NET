using CoffeePeek.Shops.Domain.Aggregates.CommunityReactionAggregate;
using CoffeePeek.Shops.Persistance.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Persistance.Repositories;

public class CommunityReactionRepository(ShopsDbContext dbContext) : ICommunityReactionRepository
{
    public void Add(CommunityReaction reaction) => dbContext.CommunityReactions.Add(reaction);

    public void Remove(CommunityReaction reaction) => dbContext.CommunityReactions.Remove(reaction);

    public Task<CommunityReaction?> GetByUserAndTargetAsync(
        Guid userId,
        ReactionTargetType targetType,
        Guid targetId,
        CancellationToken ct = default) =>
        dbContext.CommunityReactions
            .FirstOrDefaultAsync(
                r => r.UserId == userId && r.TargetType == targetType && r.TargetId == targetId,
                ct);

    public Task RemoveByTargetAsync(ReactionTargetType targetType, Guid targetId, CancellationToken ct = default) =>
        dbContext.CommunityReactions
            .Where(r => r.TargetType == targetType && r.TargetId == targetId)
            .ExecuteDeleteAsync(ct);
}
