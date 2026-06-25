using CoffeePeek.Shops.Domain.Aggregates.CommunityCommentAggregate;
using CoffeePeek.Shops.Persistance.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Persistance.Repositories;

public class QueryCommunityCommentRepository(ShopsDbContext dbContext) : IQueryCommunityCommentRepository
{
    public async Task<(IReadOnlyList<CommunityComment> TopLevelComments, int TotalCount)> GetThreadPageAsync(
        CommentTargetType targetType,
        Guid targetId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = dbContext.CommunityComments
            .AsNoTracking()
            .Where(c => !c.IsSoftDelete
                        && c.TargetType == targetType
                        && c.TargetId == targetId
                        && c.ParentCommentId == null);

        var totalCount = await query.CountAsync(ct);

        var comments = await query
            .OrderByDescending(c => c.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (comments, totalCount);
    }

    public async Task<IReadOnlyList<CommunityComment>> GetRepliesByParentIdsAsync(
        IEnumerable<Guid> parentCommentIds,
        CancellationToken ct = default)
    {
        var parentIds = parentCommentIds.Distinct().ToArray();
        if (parentIds.Length == 0)
            return [];

        return await dbContext.CommunityComments
            .AsNoTracking()
            .Where(c => !c.IsSoftDelete
                        && c.ParentCommentId != null
                        && parentIds.Contains(c.ParentCommentId.Value))
            .OrderBy(c => c.CreatedAtUtc)
            .ToListAsync(ct);
    }

    public async Task<Dictionary<Guid, int>> GetCommentCountsByTargetsAsync(
        CommentTargetType targetType,
        IEnumerable<Guid> targetIds,
        CancellationToken ct = default)
    {
        var ids = targetIds.Distinct().ToArray();
        if (ids.Length == 0)
            return new Dictionary<Guid, int>();

        return await dbContext.CommunityComments
            .AsNoTracking()
            .Where(c => !c.IsSoftDelete
                        && c.ParentCommentId == null
                        && c.TargetType == targetType
                        && ids.Contains(c.TargetId))
            .GroupBy(c => c.TargetId)
            .Select(g => new { TargetId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TargetId, x => x.Count, ct);
    }

    public async Task SoftDeleteRepliesAsync(Guid parentCommentId, CancellationToken ct = default)
    {
        await dbContext.CommunityComments
            .Where(c => c.ParentCommentId == parentCommentId && !c.IsSoftDelete)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(c => c.IsSoftDelete, true),
                ct);
    }

    public Task SoftDeleteByTargetAsync(CommentTargetType targetType, Guid targetId, CancellationToken ct = default) =>
        dbContext.CommunityComments
            .Where(c => !c.IsSoftDelete && c.TargetType == targetType && c.TargetId == targetId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(c => c.IsSoftDelete, true),
                ct);
}
