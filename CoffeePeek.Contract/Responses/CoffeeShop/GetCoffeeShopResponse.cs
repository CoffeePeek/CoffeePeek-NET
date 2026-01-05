using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Contract.Responses.CoffeeShop;

public class GetCoffeeShopResponse(ShopDto shopDto)
{
    public ShopDto Shop { get; init; } = shopDto;
}