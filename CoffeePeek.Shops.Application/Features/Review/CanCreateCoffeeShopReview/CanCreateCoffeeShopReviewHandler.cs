using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;

namespace CoffeePeek.Shops.Application.Features.Review.CanCreateCoffeeShopReview;

public class CanCreateCoffeeShopReviewHandler
{
    public async Task<Response<CanCreateCoffeeShopReviewResponse>> Handle(CanCreateCoffeeShopReviewQuery request,
        IQueryReviewRepository reviewRepository,
        CancellationToken cancellationToken)
    {
        var reviewId = await reviewRepository.ExistsForCurrentUser(request.ShopId, request.UserId, cancellationToken);

        var response = new CanCreateCoffeeShopReviewResponse(reviewId == null, reviewId);
        return Response<CanCreateCoffeeShopReviewResponse>.Success(response);
    }
}