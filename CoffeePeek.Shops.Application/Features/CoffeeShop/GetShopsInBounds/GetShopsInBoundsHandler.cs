using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.GetShopsInBounds;

public class GetShopsInBoundsHandler(IGenericRepository<Domain.Aggregates.CoffeeShopAggregate.CoffeeShop> shopRepository)
    : IRequestHandler<GetShopsInBoundsQuery, Response<GetShopsInBoundsResponse>>
{
    private const int MaxShopsInBoundMap = 500;
    public async Task<Response<GetShopsInBoundsResponse>> Handle(GetShopsInBoundsQuery query, CancellationToken cancellationToken)
    {
        var shops = await shopRepository
                .QueryAsNoTracking()
                .Where(s =>
                             s.Location.Latitude.HasValue &&
                             s.Location.Longitude.HasValue &&
                             s.Location.Latitude >= query.MinLat &&
                             s.Location.Latitude <= query.MaxLat &&
                             s.Location.Longitude >= query.MinLon &&
                             s.Location.Longitude <= query.MaxLon)
                .Select(s => new MapShopDto
                {
                    Id = s.Id,
                    Latitude = s.Location.Latitude!.Value,
                    Longitude = s.Location.Longitude!.Value,
                    Title = s.Name
                })
                .Take(MaxShopsInBoundMap)
                .ToArrayAsync(cancellationToken);

        var response = new GetShopsInBoundsResponse(shops);
        return Response<GetShopsInBoundsResponse>.Success(response);
    }
}