using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.ShopsService.Abstractions;
using CoffeePeek.ShopsService.Abstractions.ValidationStrategy;
using CoffeePeek.ShopsService.DB;
using MediatR;

namespace CoffeePeek.ShopsService.Handlers.CoffeeShop.Review;

public class AddCoffeeShopReviewRequestHandler(ShopsDbContext dbContext, 
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

        var review = new Entities.Review
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