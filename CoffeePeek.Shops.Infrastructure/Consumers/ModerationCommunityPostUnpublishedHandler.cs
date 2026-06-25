using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shops.Application.Features.Public.Feed;
using CoffeePeek.Shops.Domain.Aggregates.CommunityCommentAggregate;
using CoffeePeek.Shops.Domain.Aggregates.CommunityPostAggregate;
using CoffeePeek.Shops.Domain.Aggregates.CommunityReactionAggregate;

namespace CoffeePeek.Shops.Infrastructure.Consumers;

public static class ModerationCommunityPostUnpublishedHandler
{
    public static async Task Handle(
        ModerationCommunityPostUnpublishedEvent @event,
        ICommunityPostRepository postRepository,
        IQueryCommunityCommentRepository commentRepository,
        ICommunityReactionRepository reactionRepository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        CancellationToken ct)
    {
        var post = await postRepository.GetByModerationPostIdAsync(@event.ModerationPostId, ct);
        if (post is null)
            return;

        post.SoftDelete();
        await commentRepository.SoftDeleteByTargetAsync(CommentTargetType.Post, post.Id, ct);
        await reactionRepository.RemoveByTargetAsync(ReactionTargetType.Post, post.Id, ct);
        await unitOfWork.SaveChangesAsync(ct);
        await CommunityFeedCacheInvalidator.InvalidateAsync(cacheService, ct);
    }
}
