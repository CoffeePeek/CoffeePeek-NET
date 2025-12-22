using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Application.Handlers.CoffeeShop;

public class GetShopsInBoundsHandler(IGenericRepository<Shop> shopRepository)
    : IRequestHandler<GetShopsInBoundsRequest, Response<GetShopsInBoundsResponse>>
{
    public async Task<Response<GetShopsInBoundsResponse>> Handle(GetShopsInBoundsRequest request, CancellationToken cancellationToken)
    {
        var shops = await shopRepository
                .QueryAsNoTracking()
                .Where(s => s.Location != null &&
                             s.Location.Latitude.HasValue &&
                             s.Location.Longitude.HasValue &&
                             s.Location.Latitude >= request.MinLat &&
                             s.Location.Latitude <= request.MaxLat &&
                             s.Location.Longitude >= request.MinLon &&
                             s.Location.Longitude <= request.MaxLon)
                .Select(s => new MapShopDto
                {
                    Id = s.Id,
                    Latitude = s.Location!.Latitude!.Value,
                    Longitude = s.Location!.Longitude!.Value,
                    Title = s.Name
                })
                .Take(500)
                .ToArrayAsync(cancellationToken);
            

        var response = new GetShopsInBoundsResponse(shops);
        return Response<GetShopsInBoundsResponse>.Success(response);
    }
}

