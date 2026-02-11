using System.Net;
using CoffeePeek.Moderation.Domain.Aggregates.ModerationReviewAggregate;
using CoffeePeek.Shared.Kernel.Response;
using Wolverine.Attributes;

namespace CoffeePeek.Moderation.Application.Features.Review.UpdateCoffeeShopReview;

public static class UpdateCoffeeShopReviewHandler
{
    [Transactional]
    public static async Task<Response<UpdateCoffeeShopReviewResponse>> Handle(
        UpdateCoffeeShopReviewCommand command,
        IModerationReviewRepository reviewRepository,
        CancellationToken ct)
    {
        var review = await reviewRepository.GetById(command.ReviewId, ct);

        if (review == null)
        {
            return Response<UpdateCoffeeShopReviewResponse>.Error(
                HttpStatusCode.NotFound, "Review not found");
        }

        if (review.UserId != command.UserId)
        {
            return Response<UpdateCoffeeShopReviewResponse>.Error(
                HttpStatusCode.Forbidden, "You are not authorized to update this review");
        }

        review.UpdateHeader(command.Header);
        review.UpdateComment(command.Comment);
        review.Rating.UpdateRating(
            command.Rating.Place, 
            command.Rating.Service, 
            command.Rating.Coffee);

        return Response<UpdateCoffeeShopReviewResponse>.Success(new UpdateCoffeeShopReviewResponse(review.Id));
    }
}