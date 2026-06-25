using CoffeePeek.Shops.Domain.Aggregates.CommunityCommentAggregate;

namespace CoffeePeek.Shops.Domain.Aggregates.CommunityCommentAggregate;

public interface ICommunityCommentRepository
{
    void Add(CommunityComment comment);
    Task<CommunityComment?> GetById(Guid commentId, CancellationToken ct = default);
}

public interface IQueryCommunityCommentRepository
{
    Task<(IReadOnlyList<CommunityComment> TopLevelComments, int TotalCount)> GetThreadPageAsync(
        CommentTargetType targetType,
        Guid targetId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<IReadOnlyList<CommunityComment>> GetRepliesByParentIdsAsync(
        IEnumerable<Guid> parentCommentIds,
        CancellationToken ct = default);

    Task<Dictionary<Guid, int>> GetCommentCountsByTargetsAsync(
        CommentTargetType targetType,
        IEnumerable<Guid> targetIds,
        CancellationToken ct = default);

    Task SoftDeleteRepliesAsync(Guid parentCommentId, CancellationToken ct = default);
}
