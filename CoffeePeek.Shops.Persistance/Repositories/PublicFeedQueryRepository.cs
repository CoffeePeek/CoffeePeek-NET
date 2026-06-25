using CoffeePeek.Contract.Dtos.Public;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Shops.Application.Features.Public.Feed;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Persistance.Configuration;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using CheckIn = CoffeePeek.Shops.Domain.Aggregates.CheckInAggregate.CheckIn;
using Review = CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate.Review;

namespace CoffeePeek.Shops.Persistance.Repositories;

public class PublicFeedQueryRepository(
    ShopsDbContext dbContext,
    IMapper mapper,
    IQueryCoffeeShopRepository coffeeShopRepository) : ICommunityFeedQueries
{
    public async Task<(IReadOnlyList<CommunityFeedItemDto> Items, int TotalCount)> GetFeedAsync(
        int page,
        int pageSize,
        CommunityFeedFilter filter,
        CancellationToken cancellationToken = default)
    {
        return filter switch
        {
            CommunityFeedFilter.Reviews => await GetReviewsFeedAsync(page, pageSize, cancellationToken),
            CommunityFeedFilter.CheckIns => await GetCheckInsFeedAsync(page, pageSize, cancellationToken),
            _ => await GetMergedFeedAsync(page, pageSize, cancellationToken)
        };
    }

    private async Task<(IReadOnlyList<CommunityFeedItemDto> Items, int TotalCount)> GetReviewsFeedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Reviews
            .AsNoTracking()
            .Where(r => !r.IsSoftDelete);

        var totalCount = await query.CountAsync(cancellationToken);

        var reviews = await query
            .OrderByDescending(r => r.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(r => r.Photos)
            .ToListAsync(cancellationToken);

        var items = await EnrichShopNamesAsync(
            reviews.Select(r => mapper.Map<CommunityFeedItemDto>(r)).ToList(),
            cancellationToken);

        return (items, totalCount);
    }

    private async Task<(IReadOnlyList<CommunityFeedItemDto> Items, int TotalCount)> GetCheckInsFeedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = dbContext.CheckIns.AsNoTracking();
        var totalCount = await query.CountAsync(cancellationToken);

        var checkIns = await query
            .OrderByDescending(c => c.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(c => c.ShopPhotos)
            .ToListAsync(cancellationToken);

        var items = await EnrichShopNamesAsync(
            checkIns.Select(c => mapper.Map<CommunityFeedItemDto>(c)).ToList(),
            cancellationToken);

        return (items, totalCount);
    }

    private async Task<(IReadOnlyList<CommunityFeedItemDto> Items, int TotalCount)> GetMergedFeedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var reviewCount = await dbContext.Reviews
            .AsNoTracking()
            .CountAsync(r => !r.IsSoftDelete, cancellationToken);
        var checkInCount = await dbContext.CheckIns.AsNoTracking().CountAsync(cancellationToken);
        var totalCount = reviewCount + checkInCount;

        if (totalCount == 0)
            return ([], 0);

        var window = page * pageSize;

        var reviews = await dbContext.Reviews
            .AsNoTracking()
            .Where(r => !r.IsSoftDelete)
            .OrderByDescending(r => r.CreatedAtUtc)
            .Take(window)
            .Include(r => r.Photos)
            .ToListAsync(cancellationToken);

        var checkIns = await dbContext.CheckIns
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAtUtc)
            .Take(window)
            .Include(c => c.ShopPhotos)
            .ToListAsync(cancellationToken);

        var merged = reviews
            .Select(r => mapper.Map<CommunityFeedItemDto>(r))
            .Concat(checkIns.Select(c => mapper.Map<CommunityFeedItemDto>(c)))
            .OrderByDescending(item => item.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var items = await EnrichShopNamesAsync(merged, cancellationToken);
        return (items, totalCount);
    }

    private async Task<IReadOnlyList<CommunityFeedItemDto>> EnrichShopNamesAsync(
        IReadOnlyList<CommunityFeedItemDto> items,
        CancellationToken cancellationToken)
    {
        if (items.Count == 0)
            return items;

        var shopIds = items.Select(i => i.ShopId).Distinct();
        var shopNames = await coffeeShopRepository.GetShopNamesByIdsAsync(shopIds, cancellationToken);

        return items
            .Select(item => item with
            {
                ShopName = shopNames.GetValueOrDefault(item.ShopId, string.Empty)
            })
            .ToList();
    }
}
