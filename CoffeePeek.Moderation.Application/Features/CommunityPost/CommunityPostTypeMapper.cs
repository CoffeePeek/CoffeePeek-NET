using CoffeePeek.Contract.Enums;
using CoffeePeek.Moderation.Domain.Aggregates.ModerationCommunityPostAggregate;

namespace CoffeePeek.Moderation.Application.Features.CommunityPost;

public static class CommunityPostTypeMapper
{
    public static ModerationCommunityPostType ToDomain(CommunityPostType postType) =>
        postType switch
        {
            CommunityPostType.Discussion => ModerationCommunityPostType.Discussion,
            CommunityPostType.Question => ModerationCommunityPostType.Question,
            CommunityPostType.Tip => ModerationCommunityPostType.Tip,
            _ => throw new ArgumentOutOfRangeException(nameof(postType), postType, null)
        };

    public static CommunityPostType ToContract(ModerationCommunityPostType postType) =>
        postType switch
        {
            ModerationCommunityPostType.Discussion => CommunityPostType.Discussion,
            ModerationCommunityPostType.Question => CommunityPostType.Question,
            ModerationCommunityPostType.Tip => CommunityPostType.Tip,
            _ => throw new ArgumentOutOfRangeException(nameof(postType), postType, null)
        };
}
