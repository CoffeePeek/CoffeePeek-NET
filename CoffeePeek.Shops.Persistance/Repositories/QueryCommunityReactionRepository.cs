using CoffeePeek.Shops.Domain.Aggregates.CommunityReactionAggregate;
using CoffeePeek.Shops.Persistance.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Persistance.Repositories;

public class QueryCommunityReactionRepository(ShopsDbContext dbContext) : IQueryCommunityReactionRepository
{
    public async Task<Dictionary<Guid, CommunityReactionCounts>> GetCountsByTargetsAsync(
        ReactionTargetType targetType,
        IEnumerable<Guid> targetIds,
        CancellationToken ct = default)
    {
        var ids = targetIds.Distinct().ToArray();
        if (ids.Length == 0)
            return [];

        var rows = await dbContext.CommunityReactions
            .AsNoTracking()
            .Where(r => r.TargetType == targetType && ids.Contains(r.TargetId))
            .GroupBy(r => r.TargetId)
            .Select(g => new
            {
                TargetId = g.Key,
                WantToTry = g.Count(x => x.ReactionType == CommunityReactionType.WantToTry),
                GreatFind = g.Count(x => x.ReactionType == CommunityReactionType.GreatFind),
                Helpful = g.Count(x => x.ReactionType == CommunityReactionType.Helpful)
            })
            .ToListAsync(ct);

        return rows.ToDictionary(
            x => x.TargetId,
            x => new CommunityReactionCounts(x.WantToTry, x.GreatFind, x.Helpful));
    }

    public async Task<Dictionary<Guid, CommunityReactionType>> GetViewerReactionsByTargetsAsync(
        Guid viewerUserId,
        ReactionTargetType targetType,
        IEnumerable<Guid> targetIds,
        CancellationToken ct = default)
    {
        var ids = targetIds.Distinct().ToArray();
        if (ids.Length == 0)
            return [];

        return await dbContext.CommunityReactions
            .AsNoTracking()
            .Where(r => r.UserId == viewerUserId && r.TargetType == targetType && ids.Contains(r.TargetId))
            .ToDictionaryAsync(r => r.TargetId, r => r.ReactionType, ct);
    }
}
