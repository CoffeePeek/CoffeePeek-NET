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
    IValidationStrategy<UpdateCoffeeShopReviewCommand> validationStrategy)
    : IRequestHandler<UpdateCoffeeShopReviewCommand, Response<UpdateCoffeeShopReviewResponse>>
{
    public async Task<Response<UpdateCoffeeShopReviewResponse>> Handle(UpdateCoffeeShopReviewCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = validationStrategy.Validate(command);
        if (!validationResult.IsValid)
        {
            return Response<UpdateCoffeeShopReviewResponse>.Error(HttpStatusCode.BadRequest,
                validationResult.ErrorMessage);
        }

        var review = await reviewRepository.FirstOrDefaultAsync(x => x.Id == command.ReviewId, cancellationToken);

        if (review == null)
        {
            return Response<UpdateCoffeeShopReviewResponse>.Error(HttpStatusCode.NotFound, "Review not found");
        }

        if (review.UserId != command.UserId)
        {
            return Response<UpdateCoffeeShopReviewResponse>.Error(HttpStatusCode.Forbidden,
                "You are not authorized to update this review");
        }


        review.UpdateHeader(command.Header);
        review.UpdateComment(command.Comment);
        review.Rating.UpdateRating(command.Rating.Place, command.Rating.Service, command.Rating.Coffee);


        reviewRepository.Update(review);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Response<UpdateCoffeeShopReviewResponse>.Success(new UpdateCoffeeShopReviewResponse(review.Id));
    }
}