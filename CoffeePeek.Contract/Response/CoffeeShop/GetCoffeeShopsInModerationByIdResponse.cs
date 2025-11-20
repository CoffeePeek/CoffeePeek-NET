using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Contract.Response.CoffeeShop;

public class GetCoffeeShopsInModerationByIdResponse
{
    public GetCoffeeShopsInModerationByIdResponse(ModerationShopDto[] reviewShops)
    {
        ReviewShops = reviewShops;
    }

    public ModerationShopDto[] ReviewShops { get; }
}