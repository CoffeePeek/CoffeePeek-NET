using CoffeePeek.Contract.Abstract;
using CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Review.CanCreateCoffeeShopReview;

public class CanCreateCoffeeShopReviewHandler(IReviewRepository reviewRepository)
    : IRequestHandler<CanCreateCoffeeShopReviewQuery, Response<CanCreateCoffeeShopReviewResponse>>
{
    public async Task<Response<CanCreateCoffeeShopReviewResponse>> Handle(CanCreateCoffeeShopReviewQuery request,
        CancellationToken cancellationToken)
    {
        var (exists, reviewId) = await reviewRepository.ExistsForCurrentUser(request.ShopId, request.UserId, cancellationToken);

        var response = new CanCreateCoffeeShopReviewResponse(!exists, reviewId);
        return Response<CanCreateCoffeeShopReviewResponse>.Success(response);
    }
}