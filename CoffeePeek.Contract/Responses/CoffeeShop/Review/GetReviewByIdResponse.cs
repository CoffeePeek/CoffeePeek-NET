using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Contract.Responses.CoffeeShop.Review;

public class GetReviewByIdResponse(CoffeeShopReviewDto review)
{
    public CoffeeShopReviewDto Review { get; } = review;
}