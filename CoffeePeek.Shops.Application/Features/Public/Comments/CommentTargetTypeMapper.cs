using CoffeePeek.Contract.Enums;
using CoffeePeek.Shops.Domain.Aggregates.CommunityCommentAggregate;

namespace CoffeePeek.Shops.Application.Features.Public.Comments;

public static class CommentTargetTypeMapper
{
    public static CommentTargetType ToDomain(CommunityCommentTargetType targetType) =>
        targetType switch
        {
            CommunityCommentTargetType.Review => CommentTargetType.Review,
            CommunityCommentTargetType.CheckIn => CommentTargetType.CheckIn,
            _ => throw new ArgumentOutOfRangeException(nameof(targetType), targetType, null)
        };

    public static CommunityCommentTargetType ToContract(CommentTargetType targetType) =>
        targetType switch
        {
            CommentTargetType.Review => CommunityCommentTargetType.Review,
            CommentTargetType.CheckIn => CommunityCommentTargetType.CheckIn,
            _ => throw new ArgumentOutOfRangeException(nameof(targetType), targetType, null)
        };
}
