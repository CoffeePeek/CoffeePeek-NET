using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Contract.Responses.CoffeeShop.Review;

public class GetReviewByIdResponse(ReviewDto review)
{
    public ReviewDto Review { get; } = review;
}