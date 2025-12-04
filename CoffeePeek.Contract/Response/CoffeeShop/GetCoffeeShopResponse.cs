using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Contract.Response.CoffeeShop;

public class GetCoffeeShopResponse(ShopDto shopDto)
{
    public ShopDto Shop { get; init; } = shopDto;
}