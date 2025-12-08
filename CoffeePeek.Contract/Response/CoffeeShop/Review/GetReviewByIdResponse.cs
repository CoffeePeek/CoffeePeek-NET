using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Contract.Response.CoffeeShop;

public class GetReviewByIdResponse(CoffeeShopReviewDto review)
{
    public CoffeeShopReviewDto Review { get; } = review;
}