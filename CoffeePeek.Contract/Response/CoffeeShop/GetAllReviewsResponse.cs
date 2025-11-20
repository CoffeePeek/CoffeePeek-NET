using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Contract.Response.CoffeeShop;

public class GetAllReviewsResponse(ICollection<CoffeeShopReviewDto> reviews)
{
    public ICollection<CoffeeShopReviewDto> Reviews { get; } = reviews;
}