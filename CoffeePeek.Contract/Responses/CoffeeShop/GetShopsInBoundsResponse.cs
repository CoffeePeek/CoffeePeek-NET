using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Contract.Responses.CoffeeShop;

public class GetShopsInBoundsResponse
{
    public GetShopsInBoundsResponse(IEnumerable<MapShopDto> shops)
    {
        Shops = shops.ToArray();
    }

    public MapShopDto[] Shops { get; set; }
}

