using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Contract.Response.CoffeeShop.Review;

public class GetReviewsByUserIdResponse(CoffeeShopReviewDto[] reviewDtos)
{
    public CoffeeShopReviewDto[] Reviews { get; init; } = reviewDtos;
}