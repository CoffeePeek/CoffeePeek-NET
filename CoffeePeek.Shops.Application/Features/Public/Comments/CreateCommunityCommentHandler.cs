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
using CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;

namespace CoffeePeek.Shops.Application.Features.Public.Comments;

public static class CreateCommunityCommentHandler
{
    public static async Task<(Response<CreateCommunityCommentResponse>, CommunityCommentNotificationEvent?)> Handle(
        CreateCommunityCommentCommand command,
        ICommunityCommentRepository commentRepository,
        IQueryReviewRepository reviewRepository,
        IQueryCheckInRepository checkInRepository,
        IQueryCommunityPostRepository postRepository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        CancellationToken ct)
    {
        var domainTargetType = CommentTargetTypeMapper.ToDomain(command.TargetType);

        if (!await TargetExistsAsync(domainTargetType, command.TargetId, reviewRepository, checkInRepository, postRepository, ct))
            throw new NotFoundException("Feed item not found.");

        Guid? parentCommentId = null;
        CommunityComment? parentComment = null;
        if (command.ParentCommentId is { } parentIdInput)
        {
            parentComment = await commentRepository.GetById(parentIdInput, ct);
            if (parentComment is null || parentComment.IsSoftDelete)
                throw new NotFoundException("Parent comment not found.");

            if (parentComment.ParentCommentId is not null)
                throw new DomainException("Replies are limited to one level deep.");

            if (parentComment.TargetType != domainTargetType || parentComment.TargetId != command.TargetId)
                throw new DomainException("Parent comment does not belong to the same feed item.");

            parentCommentId = parentComment.Id;
        }

        var comment = CommunityComment.Create(
            command.UserId,
            command.UserName,
            command.Body,
            domainTargetType,
            command.TargetId,
            parentCommentId);

        commentRepository.Add(comment);
        await unitOfWork.SaveChangesAsync(ct);
        await CommunityFeedCacheInvalidator.InvalidateAsync(cacheService, ct);

        var response = Response<CreateCommunityCommentResponse>.Success(new CreateCommunityCommentResponse(comment.Id));

        var authorUserId = await CommunityTargetAuthorResolver.ResolveAuthorUserIdAsync(
            domainTargetType,
            command.TargetId,
            reviewRepository,
            checkInRepository,
            postRepository,
            ct);

        CommunityCommentNotificationEvent? notificationEvent = null;
        if (parentComment is not null && parentComment.UserId != command.UserId)
        {
            notificationEvent = new CommunityCommentNotificationEvent(
                parentComment.UserId,
                command.UserId,
                command.UserName,
                command.TargetType,
                command.TargetId,
                comment.Id);
        }
        else if (authorUserId is { } recipientId && recipientId != command.UserId)
        {
            notificationEvent = new CommunityCommentNotificationEvent(
                recipientId,
                command.UserId,
                command.UserName,
                command.TargetType,
                command.TargetId,
                comment.Id);
        }

        return (response, notificationEvent);
    }

    private static async Task<bool> TargetExistsAsync(
        CommentTargetType targetType,
        Guid targetId,
        IQueryReviewRepository reviewRepository,
        IQueryCheckInRepository checkInRepository,
        IQueryCommunityPostRepository postRepository,
        CancellationToken ct) =>
        targetType switch
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
