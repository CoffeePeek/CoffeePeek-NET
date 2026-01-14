using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.GetShopsInBounds;

public class GetShopsInBoundsResponse(IEnumerable<MapShopDto> shops)
{
    public MapShopDto[] Shops { get; set; } = shops.ToArray();
}