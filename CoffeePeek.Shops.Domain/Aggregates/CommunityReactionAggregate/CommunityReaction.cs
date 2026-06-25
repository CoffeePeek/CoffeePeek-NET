using CoffeePeek.Shared.Domain.Entities;

namespace CoffeePeek.Shops.Domain.Aggregates.CommunityReactionAggregate;

public sealed class CommunityReaction : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public ReactionTargetType TargetType { get; private set; }
    public Guid TargetId { get; private set; }
    public CommunityReactionType ReactionType { get; private set; }

    // ReSharper disable once UnusedMember.Local
    private CommunityReaction() { }

    private CommunityReaction(Guid userId, ReactionTargetType targetType, Guid targetId, CommunityReactionType reactionType)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        TargetType = targetType;
        TargetId = targetId;
        ReactionType = reactionType;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static CommunityReaction Create(
        Guid userId,
        ReactionTargetType targetType,
        Guid targetId,
        CommunityReactionType reactionType) =>
        new(userId, targetType, targetId, reactionType);

    public void ChangeType(CommunityReactionType reactionType) => ReactionType = reactionType;
}
