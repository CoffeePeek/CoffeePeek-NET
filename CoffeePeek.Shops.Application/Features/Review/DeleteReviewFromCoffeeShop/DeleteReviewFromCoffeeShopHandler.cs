using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;

namespace CoffeePeek.Shops.Application.Features.Review.DeleteReviewFromCoffeeShop;

public class DeleteReviewFromCoffeeShopHandler
{
    public async Task<Response> Handle(DeleteReviewFromCoffeeShopCommand request, 
        IReviewRepository reviewRepository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var review = await reviewRepository.GetById(request.ReviewId, cancellationToken);

        if (review == null)
        {
            throw new NotFoundException($"{nameof(Review)} not found by id");
        }

        review.SoftDelete();

        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Response.Success();
    }
}