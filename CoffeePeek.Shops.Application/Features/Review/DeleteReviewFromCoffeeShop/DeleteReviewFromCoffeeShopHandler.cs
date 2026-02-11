using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;
using Wolverine.Attributes;

namespace CoffeePeek.Shops.Application.Features.Review.DeleteReviewFromCoffeeShop;

public class DeleteReviewFromCoffeeShopHandler
{
    [Transactional]
    public async Task<Response> Handle(DeleteReviewFromCoffeeShopCommand request, 
        IReviewRepository reviewRepository,
        CancellationToken cancellationToken)
    {
        var review = await reviewRepository.GetById(request.ReviewId, cancellationToken);

        if (review == null)
        {
            throw new NotFoundException($"{nameof(Review)} not found by id");
        }

        review.SoftDelete();

        return Response.Success();
    }
}