using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop.Favorite;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Application.Commands.CoffeeShop.Favorite;
using CoffeePeek.Shops.Domain.Entities;
using MapsterMapper;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.Favorite;

public class GetAllFavoritesHandler(IGenericRepository<FavoriteShop> favoriteShopRepository, IMapper mapper) : IRequestHandler<GetAllFavoritesCommand, Response<GetAllFavoritesResponse>>
{
    public async Task<Response<GetAllFavoritesResponse>> Handle(GetAllFavoritesCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            return Response<GetAllFavoritesResponse>.Error("UserId is required and cannot be empty");
        }

        

        var result = new GetAllFavoritesResponse([]);
        
        return Response<GetAllFavoritesResponse>.Success(result);
    }
}