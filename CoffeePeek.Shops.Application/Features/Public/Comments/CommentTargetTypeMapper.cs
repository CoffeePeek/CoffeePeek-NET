using CoffeePeek.Contract.Enums;
using CoffeePeek.Shops.Domain.Aggregates.CommunityCommentAggregate;
using CoffeePeek.Shops.Domain.Aggregates.CommunityReactionAggregate;

namespace CoffeePeek.Shops.Application.Features.Public.Comments;

public static class CommentTargetTypeMapper
{
    public static CommentTargetType ToDomain(CommunityCommentTargetType targetType) =>
        targetType switch
        {
            CommunityCommentTargetType.Review => CommentTargetType.Review,
            CommunityCommentTargetType.CheckIn => CommentTargetType.CheckIn,
            CommunityCommentTargetType.Post => CommentTargetType.Post,
            _ => throw new ArgumentOutOfRangeException(nameof(targetType), targetType, null)
        };

    public static CommunityCommentTargetType ToContract(CommentTargetType targetType) =>
        targetType switch
        {
            CommentTargetType.Review => CommunityCommentTargetType.Review,
            CommentTargetType.CheckIn => CommunityCommentTargetType.CheckIn,
            CommentTargetType.Post => CommunityCommentTargetType.Post,
            _ => throw new ArgumentOutOfRangeException(nameof(targetType), targetType, null)
        };

    public static ReactionTargetType ToReactionTarget(CommunityCommentTargetType targetType) =>
        targetType switch
        {
            CommunityCommentTargetType.Review => ReactionTargetType.Review,
            CommunityCommentTargetType.CheckIn => ReactionTargetType.CheckIn,
            CommunityCommentTargetType.Post => ReactionTargetType.Post,
            _ => throw new ArgumentOutOfRangeException(nameof(targetType), targetType, null)
        };

    public static CommunityCommentTargetType ToContract(ReactionTargetType targetType) =>
        targetType switch
        {
            ReactionTargetType.Review => CommunityCommentTargetType.Review,
            ReactionTargetType.CheckIn => CommunityCommentTargetType.CheckIn,
            ReactionTargetType.Post => CommunityCommentTargetType.Post,
            _ => throw new ArgumentOutOfRangeException(nameof(targetType), targetType, null)
        };
}
