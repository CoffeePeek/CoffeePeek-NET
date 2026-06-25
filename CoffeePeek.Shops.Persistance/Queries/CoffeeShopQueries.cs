using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShop;
using CoffeePeek.Shops.Application.Features.CoffeeShop.GetShopsInBounds;
using CoffeePeek.Shops.Application.Features.CoffeeShop.SearchCoffeeShops;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Persistance.Configuration;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Persistance.Queries;

public class CoffeeShopQueries(ShopsDbContext context, IMapper mapper) : ICoffeeShopQueries
{
    public async Task<(ShortShopDto[] Items, int TotalCount)> Search(SearchCoffeeShopsQuery request, CancellationToken ct)
    {
        var query = context.Shops.AsNoTracking()
            .Where(s => s.Status == CoffeeShopStatus.Active);
        
        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var term = $"%{request.Query.Trim()}%";
            query = query.Where(s => EF.Functions.ILike(s.Name, term) || EF.Functions.ILike(s.Location.Address, term));
        }

        if (request.CityId.HasValue)
        {
            query = query.Where(s => s.Location.CityId == request.CityId.Value);
        }

        if (request.Equipments is { Length: > 0 })
        {
            query = query.Where(s => s.Equipments.Any(se => request.Equipments.Contains(se.Id)));
        }

        if (request.Beans is { Length: > 0 })
        {
            query = query.Where(s =>
                s.CoffeeBeans.Any(cbs => request.Beans.Contains(cbs.Id)));
        }

        if (request.Roasters is { Length: > 0 })
        {
            query = query.Where(s =>
                s.Roasters.Any(rs => request.Roasters.Contains(rs.Id)));
        }

        if (request.BrewMethods is { Length: > 0 })
        {
            query = query.Where(s =>
                s.BrewMethods.Any(sbm => request.BrewMethods.Contains(sbm.Id)));
        }

        if (request.PriceRange.HasValue)
        {
            var priceRangeValue = (int)request.PriceRange.Value;
            query = query.Where(s => (int)s.PriceRange == priceRangeValue);
        }
        
        if (request.MinRating.HasValue)
        {
            var minRating = request.MinRating.Value;
            var ratingSubquery = context.Reviews
                .Where(r => !r.IsSoftDelete)
                .GroupBy(r => r.CoffeeShopId)
                .Select(g => new { CoffeeShopId = g.Key, Avg = g.Average(r => r.Rating.AverageRating) });
            // INNER JOIN: shops with no reviews are excluded when MinRating filter is active — consistent with previous behavior
            query = query
                .Join(ratingSubquery, s => s.Id, r => r.CoffeeShopId, (s, r) => new { Shop = s, r.Avg })
                .Where(x => x.Avg >= minRating)
                .Select(x => x.Shop);
        }
        
        var totalCount = await query.CountAsync(ct);
        
        var items = await query
            .AsSplitQuery()
            .OrderBy(x => x.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectToType<ShortShopDto>(mapper.Config)
            .ToArrayAsync(ct);

        return (items, totalCount);
    }

    public async Task<CoffeeShopDetailsDto?> GetDetailsById(Guid id, CancellationToken ct)
    {
        return await context.Shops
            .AsNoTracking()
            .AsSplitQuery()
            .Where(x => x.Id == id)
            .ProjectToType<CoffeeShopDetailsDto>(mapper.Config)
            .FirstOrDefaultAsync(ct);
    }

    public Task<MapShopDto[]> GetShopsInBounds(GetShopsInBoundsQuery query, CancellationToken ct = default)
    {
        return context.Shops.AsNoTracking()
            .Where(s => s.Status == CoffeeShopStatus.Active &&
                        s.Location.Latitude.HasValue &&
                        s.Location.Longitude.HasValue &&
                        s.Location.Latitude >= query.MinLat &&
                        s.Location.Latitude <= query.MaxLat &&
                        s.Location.Longitude >= query.MinLon &&
                        s.Location.Longitude <= query.MaxLon)
            .Select(s => new MapShopDto
            {
                Id = s.Id,
                Latitude = s.Location!.Latitude!.Value,
                Longitude = s.Location!.Longitude!.Value,
                Title = s.Name
            })
            .Take(500)
            .ToArrayAsync(ct);
    }

    public Task<CoffeeShopDetailsDto[]> GetUserFavoriteCoffeeShops(Guid userId, CancellationToken cancellationToken)
    {
        return context.UserFavorites.AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.CoffeeShop)
            .ProjectToType<CoffeeShopDetailsDto>(mapper.Config)
            .ToArrayAsync(cancellationToken);
    }
}