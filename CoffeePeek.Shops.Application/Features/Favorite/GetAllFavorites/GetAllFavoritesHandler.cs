using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Shops.Application.Features.Favorite.GetAllFavorites;

//TODO add real implementation
public class GetAllFavoritesHandler
{
    public Task<Response<GetAllFavoritesResponse>> Handle(GetAllFavoritesCommand request,
        CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            return Task.FromResult(Response<GetAllFavoritesResponse>.Error("UserId is required and cannot be empty"));
        }

        var result = new GetAllFavoritesResponse([]);

        return Task.FromResult(Response<GetAllFavoritesResponse>.Success(result));
    }
}