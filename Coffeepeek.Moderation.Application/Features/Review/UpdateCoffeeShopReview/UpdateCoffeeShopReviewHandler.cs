using System.Net;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Moderation.Application.Features.Review.UpdateCoffeeShopReview;
using CoffeePeek.Moderation.Domain.Entities.ModerationReviewAggregate;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Validation;
using MediatR;

namespace CoffeePeek.Moderation.Application.Features.Review.UpdateCoffeeShopReview;

public class UpdateCoffeeShopReviewRequestHandler(
    IGenericRepository<ModerationReview> reviewRepository,
    IUnitOfWork unitOfWork,
    IValidationStrategy<UpdateCoffeeShopReviewRequest> validationStrategy)
    : IRequestHandler<UpdateCoffeeShopReviewRequest, Response<UpdateCoffeeShopReviewResponse>>
{
    public async Task<Response<UpdateCoffeeShopReviewResponse>> Handle(UpdateCoffeeShopReviewRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = validationStrategy.Validate(request);
        if (!validationResult.IsValid)
        {
            return Response<UpdateCoffeeShopReviewResponse>.Error(HttpStatusCode.BadRequest,
                validationResult.ErrorMessage);
        }

        var review = await reviewRepository.FirstOrDefaultAsync(x => x.Id == request.ReviewId, cancellationToken);

        if (review == null)
        {
            return Response<UpdateCoffeeShopReviewResponse>.Error(HttpStatusCode.NotFound, "Review not found");
        }

        if (review.UserId != request.UserId)
        {
            return Response<UpdateCoffeeShopReviewResponse>.Error(HttpStatusCode.Forbidden,
                "You are not authorized to update this review");
        }


        review.UpdateHeader(request.Header);
        review.UpdateComment(request.Comment);
        review.UpdateRating(request.RatingCoffee, request.RatingPlace, request.RatingService);


        reviewRepository.Update(review);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Response<UpdateCoffeeShopReviewResponse>.Success(new UpdateCoffeeShopReviewResponse(review.Id));
    }
}