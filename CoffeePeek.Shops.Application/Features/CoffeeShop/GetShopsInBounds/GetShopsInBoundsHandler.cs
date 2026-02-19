using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShop;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.GetShopsInBounds;

public class GetShopsInBoundsHandler
{
    public static async Task<Response<GetShopsInBoundsResponse>> Handle(GetShopsInBoundsQuery query, ICoffeeShopQueries shopQueries, CancellationToken cancellationToken)
    {
        var shops = await shopQueries.GetShopsInBounds(query, cancellationToken);

        var response = new GetShopsInBoundsResponse(shops);
        return Response<GetShopsInBoundsResponse>.Success(response);
    }
}