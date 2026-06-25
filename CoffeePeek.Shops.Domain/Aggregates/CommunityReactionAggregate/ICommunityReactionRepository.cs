namespace CoffeePeek.Shops.Domain.Aggregates.CommunityReactionAggregate;

public interface ICommunityReactionRepository
{
    void Add(CommunityReaction reaction);
    void Remove(CommunityReaction reaction);
    Task<CommunityReaction?> GetByUserAndTargetAsync(
        Guid userId,
        ReactionTargetType targetType,
        Guid targetId,
        CancellationToken ct = default);
}

public interface IQueryCommunityReactionRepository
{
    Task<Dictionary<Guid, CommunityReactionCounts>> GetCountsByTargetsAsync(
        ReactionTargetType targetType,
        IEnumerable<Guid> targetIds,
        CancellationToken ct = default);

    Task<Dictionary<Guid, CommunityReactionType>> GetViewerReactionsByTargetsAsync(
        Guid viewerUserId,
        ReactionTargetType targetType,
        IEnumerable<Guid> targetIds,
        CancellationToken ct = default);
}

public sealed record CommunityReactionCounts(int WantToTry, int GreatFind, int Helpful);
