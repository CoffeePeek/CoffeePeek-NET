using System.Net;
using CoffeePeek.Moderation.Domain.Aggregates.ModerationReviewAggregate;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Moderation.Application.Features.Review.UpdateCoffeeShopReview;

public static class UpdateCoffeeShopReviewHandler
{
    public static async Task<Response<UpdateCoffeeShopReviewResponse>> Handle(
        UpdateCoffeeShopReviewCommand command,
        IModerationReviewRepository reviewRepository,
        IUnitOfWork unitOfWork,
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

        await unitOfWork.SaveChangesAsync(ct);
        
        return Response<UpdateCoffeeShopReviewResponse>.Success(new UpdateCoffeeShopReviewResponse(review.Id));
    }
}