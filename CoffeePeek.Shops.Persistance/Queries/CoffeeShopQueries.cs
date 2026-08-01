using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShop;
using CoffeePeek.Shops.Application.Features.CoffeeShop.GetShopsInBounds;
using CoffeePeek.Shops.Application.Features.CoffeeShop.SearchCoffeeShops;
using CoffeePeek.Shops.Domain;
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

        if (request.Tags is { Length: > 0 })
        {
            foreach (var tagId in request.Tags.Distinct())
            {
                var capturedTagId = tagId;
                query = query.Where(s => s.ShopTags.Any(t => t.TagId == capturedTagId));
            }
        }

        if (request.IsNew.HasValue)
        {
            var cutoff = DateTime.UtcNow.AddDays(-BusinessConstants.ItNewEntityInDays);
            query = request.IsNew.Value
                ? query.Where(s => s.CreatedAtUtc >= cutoff)
                : query.Where(s => s.CreatedAtUtc < cutoff);
        }

        if (request.IsVisited.HasValue && request.UserId.HasValue)
        {
            var userId = request.UserId.Value;
            query = request.IsVisited.Value
                ? query.Where(s => context.CheckIns.Any(c => c.ShopId == s.Id && c.UserId == userId))
                : query.Where(s => !context.CheckIns.Any(c => c.ShopId == s.Id && c.UserId == userId));
        }

        // IsOpen evaluated in UTC (same convention as CoffeeShop.IsOpen domain property).
        if (request.IsOpen.HasValue)
        {
            var now = DateTime.UtcNow;
            var dow = now.DayOfWeek;
            var timeOfDay = now.TimeOfDay;

            if (request.IsOpen.Value)
            {
                query = query.Where(s =>
                    !s.Schedules.Any() ||
                    s.Schedules.Any(sch =>
                        sch.DayOfWeek == dow &&
                        !sch.IsClosed &&
                        sch.Intervals.Any(i => timeOfDay >= i.OpenTime && timeOfDay <= i.CloseTime)));
            }
            else
            {
                query = query.Where(s =>
                    s.Schedules.Any() &&
                    !s.Schedules.Any(sch =>
                        sch.DayOfWeek == dow &&
                        !sch.IsClosed &&
                        sch.Intervals.Any(i => timeOfDay >= i.OpenTime && timeOfDay <= i.CloseTime)));
            }
        }
        
        var totalCount = await query.CountAsync(ct);
        
        var items = await query
            .AsSplitQuery()
            .OrderBy(x => x.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectToType<ShortShopDto>(mapper.Config)
            .ToArrayAsync(ct);

        await PatchOpenAndNewFlagsAsync(items, ct);

        return (items, totalCount);
    }

    public async Task<CoffeeShopDetailsDto?> GetDetailsById(Guid id, CancellationToken ct)
    {
        var dto = await context.Shops
            .AsNoTracking()
            .AsSplitQuery()
            .Where(x => x.Id == id)
            .ProjectToType<CoffeeShopDetailsDto>(mapper.Config)
            .FirstOrDefaultAsync(ct);

        if (dto is null)
            return null;

        var tags = await context.CoffeeShopTags
            .AsNoTracking()
            .Where(t => t.ShopId == id && t.Tag != null && t.Tag.IsActive)
            .OrderBy(t => t.Tag!.SortOrder)
            .ThenBy(t => t.Tag!.Name)
            .Select(t => new ShopTagDto(
                t.Tag!.Id,
                t.Tag.Slug,
                t.Tag.Name,
                t.Tag.Description,
                t.Tag.SortOrder))
            .ToArrayAsync(ct);

        return dto with { Tags = tags };
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

    private async Task PatchOpenAndNewFlagsAsync(ShortShopDto[] items, CancellationToken ct)
    {
        if (items.Length == 0)
            return;

        var ids = items.Select(i => i.Id).ToArray();
        var now = DateTime.UtcNow;
        var newCutoff = now.AddDays(-BusinessConstants.ItNewEntityInDays);

        var states = await context.Shops
            .AsNoTracking()
            .Where(s => ids.Contains(s.Id))
            .Select(s => new { s.Id, s.CreatedAtUtc, s.Schedules })
            .ToListAsync(ct);

        var byId = states.ToDictionary(s => s.Id);
        foreach (var item in items)
        {
            if (!byId.TryGetValue(item.Id, out var state))
                continue;

            item.IsNew = state.CreatedAtUtc >= newCutoff;
            item.IsOpen = ComputeIsOpen(state.Schedules, now);
        }
    }

    private static bool ComputeIsOpen(IEnumerable<ShopSchedule> schedules, DateTime utcNow)
    {
        var list = schedules as IList<ShopSchedule> ?? schedules.ToList();
        if (list.Count == 0)
            return true;

        var daySchedule = list.FirstOrDefault(s => s.DayOfWeek == utcNow.DayOfWeek);
        if (daySchedule is null || daySchedule.IsClosed)
            return false;

        var currentTime = utcNow.TimeOfDay;
        return daySchedule.Intervals.Any(interval =>
            currentTime >= interval.OpenTime && currentTime <= interval.CloseTime);
    }
}
