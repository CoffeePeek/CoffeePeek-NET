using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Shops.Application.Features.Review.GetReviewById;

public class GetReviewByIdResponse(ReviewDto review)
{
    public ReviewDto Review { get; } = review;
}