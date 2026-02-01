using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShop;

public sealed class GetCoffeeShopResponse(CoffeeShopDetailsDto shopDto)
{
    public CoffeeShopDetailsDto ShopDto { get; set; } = shopDto;
}