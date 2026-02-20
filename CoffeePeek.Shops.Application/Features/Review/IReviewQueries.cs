using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Shops.Application.Features.Review;

public interface IReviewQueries
{
    Task<ReviewDto[]> GetReviewsByUserId(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken);
}