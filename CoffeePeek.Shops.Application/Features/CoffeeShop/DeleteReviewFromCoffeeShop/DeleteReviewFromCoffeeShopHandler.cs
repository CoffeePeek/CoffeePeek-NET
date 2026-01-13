using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Entities.ReviewAggregate;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.DeleteReviewFromCoffeeShop;

public class DeleteReviewFromCoffeeShopHandler(IGenericRepository<Domain.Entities.ReviewAggregate.Review> reviewRepository, IUnitOfWork unitOfWork)
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