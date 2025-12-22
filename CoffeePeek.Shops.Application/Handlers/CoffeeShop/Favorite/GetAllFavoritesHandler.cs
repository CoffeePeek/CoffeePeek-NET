using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop.Favorite;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Entities;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.ShopsService.Handlers.CoffeeShop;

public class GetAllFavoritesHandler(IGenericRepository<FavoriteShop> favoriteShopRepository, IMapper mapper) : IRequestHandler<GetAllFavoritesCommand, Response<GetAllFavoritesResponse>>
{
    public async Task<Response<GetAllFavoritesResponse>> Handle(GetAllFavoritesCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            return Response<GetAllFavoritesResponse>.Error("UserId is required and cannot be empty");
        }

        var favoriteShops = await favoriteShopRepository.QueryAsNoTracking()
            .Include(x => x.Shop)
            .Where(x => x.UserId == request.UserId)
            .Select(x => x.Shop)
            .ProjectToType<ShopDto>(mapper.Config)
            .ToListAsync(cancellationToken);

        var result = new GetAllFavoritesResponse(favoriteShops);
        
        return Response<GetAllFavoritesResponse>.Success(result);
    }
}