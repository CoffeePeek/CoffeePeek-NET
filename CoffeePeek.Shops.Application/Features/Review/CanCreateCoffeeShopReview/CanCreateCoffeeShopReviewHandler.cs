using CoffeePeek.Contract.Abstract;
using CoffeePeek.Shops.Domain.Entities.ReviewAggregate;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Review.CanCreateCoffeeShopReview;

public class CanCreateCoffeeShopReviewHandler(IReviewRepository reviewRepository)
    : IRequestHandler<CanCreateCoffeeShopReviewQuery, Response<bool>>
{
    public async Task<Response<bool>> Handle(CanCreateCoffeeShopReviewQuery request,
        CancellationToken cancellationToken)
    {
        var exists = await reviewRepository.ExistsWithCurrentUser(request.ShopId, request.UserId, cancellationToken);

        return Response<bool>.Success(!exists);
    }
}