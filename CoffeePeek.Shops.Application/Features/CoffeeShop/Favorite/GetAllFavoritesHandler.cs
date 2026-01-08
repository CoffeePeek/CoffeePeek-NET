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

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.Favorite;

public class GetAllFavoritesHandler(IGenericRepository<FavoriteShop> favoriteShopRepository, IMapper mapper) : IRequestHandler<GetAllFavoritesCommand, Response<GetAllFavoritesResponse>>
{
    public async Task<Response<GetAllFavoritesResponse>> Handle(GetAllFavoritesCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            return Response<GetAllFavoritesResponse>.Error("UserId is required and cannot be empty");
        }

        var favoriteShops = await favoriteShopRepository.QueryAsNoTracking()
            .Where(x => x.UserId == request.UserId)
            .Select(x => x.Shop)
            .Include(x => x.ShopPhotos)
            .Include(x => x.Reviews)
            .Include(x => x.ShopEquipments).ThenInclude(x => x.Equipment)
            .Include(x => x.CoffeeBeanShops).ThenInclude(x => x.CoffeeBean)
            .Include(x => x.RoasterShops).ThenInclude(x => x.Roaster)
            .Include(x => x.ShopBrewMethods).ThenInclude(x => x.BrewMethod)
            .Include(x => x.ShopContact)
            .Include(x => x.Location)
            .Include(x => x.Schedules)
            .ToListAsync(cancellationToken);
        
        var favoriteDtos = favoriteShops.Adapt<List<CoffeeShopDetailsDto>>(mapper.Config);

        var result = new GetAllFavoritesResponse(favoriteDtos);
        
        return Response<GetAllFavoritesResponse>.Success(result);
    }
}