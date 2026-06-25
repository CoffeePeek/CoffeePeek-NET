using CoffeePeek.Contract.Enums;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Application.Features.Public.Feed;
using CoffeePeek.Shops.Domain.Aggregates.CommunityCommentAggregate;
using CoffeePeek.Shops.Domain.Aggregates.CommunityReactionAggregate;
using CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;

namespace CoffeePeek.Shops.Application.Features.Review.DeleteReviewFromCoffeeShop;

public class DeleteReviewFromCoffeeShopHandler
{
    public async Task<Response> Handle(
        DeleteReviewFromCoffeeShopCommand request,
        IReviewRepository reviewRepository,
        IQueryCommunityCommentRepository commentRepository,
        ICommunityReactionRepository reactionRepository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        CancellationToken cancellationToken)
    {
        var review = await reviewRepository.GetById(request.ReviewId, cancellationToken);

        if (review == null)
            throw new NotFoundException($"{nameof(Review)} not found by id");

        if (review.UserId != request.RequestingUserId)
            throw new ForbiddenException("You do not have permission to delete this review");

        review.SoftDelete();
        await commentRepository.SoftDeleteByTargetAsync(CommentTargetType.Review, review.Id, cancellationToken);
        await reactionRepository.RemoveByTargetAsync(ReactionTargetType.Review, review.Id, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await CommunityFeedCacheInvalidator.InvalidateAsync(cacheService, cancellationToken);

        return Response.Success();
    }
}
