using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.ShopsService.Abstractions;
using CoffeePeek.ShopsService.Abstractions.ValidationStrategy;
using CoffeePeek.ShopsService.DB;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.ShopsService.Handlers.CoffeeShop.Review;

public class UpdateCoffeeShopReviewRequestHandler(
    ShopsDbContext dbContext,
    IValidationStrategy<UpdateCoffeeShopReviewRequest> validationStrategy)
    : IRequestHandler<UpdateCoffeeShopReviewRequest, Response<UpdateCoffeeShopReviewResponse>>
{
    public async Task<Response<UpdateCoffeeShopReviewResponse>> Handle(UpdateCoffeeShopReviewRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = validationStrategy.Validate(request);
        if (!validationResult.IsValid)
        {
            return Response.ErrorResponse<Response<UpdateCoffeeShopReviewResponse>>(validationResult.ErrorMessage);
        }

        var review = await dbContext.Reviews
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (review == null)
        {
            return Response.ErrorResponse<Response<UpdateCoffeeShopReviewResponse>>("Review not found");
        }

        if (review.UserId != request.UserId)
        {
            return Response.ErrorResponse<Response<UpdateCoffeeShopReviewResponse>>("You are not authorized to update this review");
        }

        review.Header = request.Header;
        review.Comment = request.Comment;
        review.RatingCoffee = request.RatingCoffee;
        review.RatingService = request.RatingService;
        review.RatingPlace = request.RatingPlace;

        dbContext.Reviews.Update(review);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Response.SuccessResponse<Response<UpdateCoffeeShopReviewResponse>>(new UpdateCoffeeShopReviewResponse(review.Id));
    }
}