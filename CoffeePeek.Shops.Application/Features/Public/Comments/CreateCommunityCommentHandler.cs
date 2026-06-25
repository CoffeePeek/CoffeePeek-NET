using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Application.Features.Public.Feed;
using CoffeePeek.Shops.Domain.Aggregates.CheckInAggregate;
using CoffeePeek.Shops.Domain.Aggregates.CommunityCommentAggregate;
using CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;

namespace CoffeePeek.Shops.Application.Features.Public.Comments;

public static class CreateCommunityCommentHandler
{
    public static async Task<Response<CreateCommunityCommentResponse>> Handle(
        CreateCommunityCommentCommand command,
        ICommunityCommentRepository commentRepository,
        IQueryReviewRepository reviewRepository,
        IQueryCheckInRepository checkInRepository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        CancellationToken ct)
    {
        var domainTargetType = CommentTargetTypeMapper.ToDomain(command.TargetType);

        if (!await TargetExistsAsync(domainTargetType, command.TargetId, reviewRepository, checkInRepository, ct))
            throw new NotFoundException("Feed item not found.");

        Guid? parentCommentId = null;
        if (command.ParentCommentId is { } parentId)
        {
            var parent = await commentRepository.GetById(parentId, ct);
            if (parent is null || parent.IsSoftDelete)
                throw new NotFoundException("Parent comment not found.");

            if (parent.ParentCommentId is not null)
                throw new DomainException("Replies are limited to one level deep.");

            if (parent.TargetType != domainTargetType || parent.TargetId != command.TargetId)
                throw new DomainException("Parent comment does not belong to the same feed item.");

            parentCommentId = parent.Id;
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

        return Response<CreateCommunityCommentResponse>.Success(new CreateCommunityCommentResponse(comment.Id));
    }

    private static async Task<bool> TargetExistsAsync(
        CommentTargetType targetType,
        Guid targetId,
        IQueryReviewRepository reviewRepository,
        IQueryCheckInRepository checkInRepository,
        CancellationToken ct) =>
        targetType switch
        {
            CommentTargetType.Review => await ReviewExistsAsync(targetId, reviewRepository, ct),
            CommentTargetType.CheckIn => await checkInRepository.ExistsByIdAsync(targetId, ct),
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
