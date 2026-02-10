using CoffeePeek.Contract.Abstract;
using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Review.DeleteReviewFromCoffeeShop;

public class DeleteReviewFromCoffeeShopHandler(IGenericRepository<Domain.Aggregates.ReviewAggregate.Review> reviewRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteReviewFromCoffeeShopCommand, Response>
{
    public async Task<Response> Handle(DeleteReviewFromCoffeeShopCommand request, CancellationToken cancellationToken)
    {
        var review = await reviewRepository.FirstOrDefaultAsync(x => x.Id == request.ReviewId, cancellationToken);

        if (review == null)
        {
            throw new NotFoundException($"{nameof(Review)} not found by id");
        }

        review.SoftDelete();

        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Response.Success();
    }
}