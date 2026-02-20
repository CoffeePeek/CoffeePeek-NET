using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShop;

namespace CoffeePeek.Shops.Application.Features.Favorite.GetAllFavorites;

public class GetAllFavoritesHandler
{
    public async Task<Response<GetAllFavoritesResponse>> Handle(GetAllFavoritesCommand request,
        ICoffeeShopQueries repository, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            return Response<GetAllFavoritesResponse>.Error("UserId is required and cannot be empty");
        }

        var dtos = await repository.GetUserFavoriteCoffeeShops(request.UserId, cancellationToken);

        var result = new GetAllFavoritesResponse(dtos);

        return Response<GetAllFavoritesResponse>.Success(result);
    }
}