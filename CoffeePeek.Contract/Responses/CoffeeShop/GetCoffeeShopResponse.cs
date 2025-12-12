using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Contract.Response.CoffeeShop;

public class GetCoffeeShopResponse
{
    public ShopDto Shop { get; init; }

    public GetCoffeeShopResponse()
    {
        Shop = new ShopDto();
    }

    public GetCoffeeShopResponse(ShopDto shopDto)
    {
        Shop = shopDto;
    }
}