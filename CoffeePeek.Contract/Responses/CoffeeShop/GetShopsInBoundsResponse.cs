using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Contract.Responses.CoffeeShop;

public class GetShopsInBoundsResponse(IEnumerable<MapShopDto> shops)
{
    public MapShopDto[] Shops { get; } = shops.ToArray();
}

