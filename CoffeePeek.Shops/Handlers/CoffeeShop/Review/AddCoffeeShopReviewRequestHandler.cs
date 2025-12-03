using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Domain.Databases;
using CoffeePeek.Shops.Abstractions;
using MediatR;

namespace CoffeePeek.Shops.Handlers.CoffeeShop.Review;

public class AddCoffeeShopReviewRequestHandler(CoffeePeekDbContext dbContext, 
    IValidationStrategy<AddCoffeeShopReviewRequest> validationStrategy) 
    : IRequestHandler<AddCoffeeShopReviewRequest, Response<AddCoffeeShopReviewResponse>>
{
    public async Task<Response<AddCoffeeShopReviewResponse>> Handle(AddCoffeeShopReviewRequest request, CancellationToken cancellationToken)
    {
        var validationResult = validationStrategy.Validate(request);
        if (!validationResult.IsValid)
        {
            return Response.ErrorResponse<Response<AddCoffeeShopReviewResponse>>(validationResult.ErrorMessage);
        }

        var review = new Domain.Entities.Review.Review
        {
            Header = request.Header,
            Comment = request.Comment,
            UserId = request.UserId,
            ShopId = request.ShopId,
            RatingCoffee = request.RatingCoffee,
            RatingPlace = request.RatingPlace,
            RatingService = request.RatingService,
        };
        
        dbContext.Reviews.Add(review);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Response.SuccessResponse<Response<AddCoffeeShopReviewResponse>>(new AddCoffeeShopReviewResponse(review.Id));
    }
}