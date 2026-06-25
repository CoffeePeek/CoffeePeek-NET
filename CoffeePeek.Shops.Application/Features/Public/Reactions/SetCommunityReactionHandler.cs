using CoffeePeek.Contract.Events.Community;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Application.Features.Public.Comments;
using CoffeePeek.Shops.Application.Features.Public.Feed;
using CoffeePeek.Shops.Domain.Aggregates.CheckInAggregate;
using CoffeePeek.Shops.Domain.Aggregates.CommunityCommentAggregate;
using CoffeePeek.Shops.Domain.Aggregates.CommunityPostAggregate;
using CoffeePeek.Shops.Domain.Aggregates.CommunityReactionAggregate;
using CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;

namespace CoffeePeek.Shops.Application.Features.Public.Reactions;

public static class SetCommunityReactionHandler
{
    public static async Task<(Response<SetCommunityReactionResponse>, CommunityReactionNotificationEvent?)> Handle(
        SetCommunityReactionCommand command,
        ICommunityReactionRepository reactionRepository,
        IQueryReviewRepository reviewRepository,
        IQueryCheckInRepository checkInRepository,
        IQueryCommunityPostRepository postRepository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        CancellationToken ct)
    {
        var targetType = CommentTargetTypeMapper.ToReactionTarget(command.TargetType);

        if (!await TargetExistsAsync(targetType, command.TargetId, reviewRepository, checkInRepository, postRepository, ct))
            throw new NotFoundException("Feed item not found.");

        var existing = await reactionRepository.GetByUserAndTargetAsync(
            command.UserId, targetType, command.TargetId, ct);

        CommunityReactionType? activeReaction = null;
        var wasRemoved = false;
        var shouldNotify = false;
        Contract.Enums.CommunityReactionType? notifiedReactionType = null;

        if (command.ReactionType is null)
        {
            if (existing is not null)
            {
                reactionRepository.Remove(existing);
                wasRemoved = true;
            }
        }
        else
        {
            var domainReactionType = CommunityReactionTypeMapper.ToDomain(command.ReactionType.Value);

            if (existing is not null)
            {
                if (existing.ReactionType == domainReactionType)
                {
                    reactionRepository.Remove(existing);
                    wasRemoved = true;
                }
                else
                {
                    existing.ChangeType(domainReactionType);
                    activeReaction = domainReactionType;
                    shouldNotify = true;
                    notifiedReactionType = command.ReactionType;
                }
            }
            else
            {
                reactionRepository.Add(CommunityReaction.Create(
                    command.UserId, targetType, command.TargetId, domainReactionType));
                activeReaction = domainReactionType;
                shouldNotify = true;
                notifiedReactionType = command.ReactionType;
            }
        }

        await unitOfWork.SaveChangesAsync(ct);
        await CommunityFeedCacheInvalidator.InvalidateAsync(cacheService, ct);

        Contract.Enums.CommunityReactionType? contractActiveReaction = activeReaction is null
            ? null
            : CommunityReactionTypeMapper.ToContract(activeReaction.Value);

        var response = Response<SetCommunityReactionResponse>.Success(
            new SetCommunityReactionResponse(contractActiveReaction, wasRemoved));

        CommunityReactionNotificationEvent? notificationEvent = null;
        if (shouldNotify && notifiedReactionType is not null)
        {
            var authorUserId = await CommunityTargetAuthorResolver.ResolveAuthorUserIdAsync(
                targetType,
                command.TargetId,
                reviewRepository,
                checkInRepository,
                postRepository,
                ct);

            if (authorUserId is { } recipientId && recipientId != command.UserId)
            {
                notificationEvent = new CommunityReactionNotificationEvent(
                    recipientId,
                    command.UserId,
                    command.UserName,
                    command.TargetType,
                    command.TargetId,
                    notifiedReactionType.Value);
            }
        }

        return (response, notificationEvent);
    }

    private static async Task<bool> TargetExistsAsync(
        ReactionTargetType targetType,
        Guid targetId,
        IQueryReviewRepository reviewRepository,
        IQueryCheckInRepository checkInRepository,
        IQueryCommunityPostRepository postRepository,
        CancellationToken ct) =>
        (CommentTargetType)(int)targetType switch
        {
            CommentTargetType.Review => await ReviewExistsAsync(targetId, reviewRepository, ct),
            CommentTargetType.CheckIn => await checkInRepository.ExistsByIdAsync(targetId, ct),
            CommentTargetType.Post => await postRepository.ExistsByIdAsync(targetId, ct),
            _ => false
        };

    private static async Task<bool> ReviewExistsAsync(
        Guid reviewId,
        IQueryReviewRepository reviewRepository,
        CancellationToken ct)
    {
        var review = await reviewRepository.GetById(reviewId, ct);
        return review is not null && !review.IsSoftDelete;
    }
}
