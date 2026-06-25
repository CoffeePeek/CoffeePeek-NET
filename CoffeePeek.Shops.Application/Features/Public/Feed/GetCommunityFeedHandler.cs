using CoffeePeek.Contract.Dtos.Public;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Application.Features.Public.Feed;

namespace CoffeePeek.Shops.Application.Features.Public.Feed;

public static class GetCommunityFeedHandler
{
    public static async Task<Response<GetCommunityFeedResponse>> Handle(
        GetCommunityFeedQuery query,
        ICommunityFeedQueries repository,
        ICacheService cacheService,
        CancellationToken ct)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);

        var cacheKey = CacheKey.Shop.PublicCommunityFeed(page, pageSize, query.Filter.ToString());
        var cached = await cacheService.GetAsync<GetCommunityFeedResponse>(cacheKey, ct);

        if (cached is not null)
            return Response<GetCommunityFeedResponse>.Success(cached);

        var (items, totalCount) = await repository.GetFeedAsync(page, pageSize, query.Filter, ct);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        var response = new GetCommunityFeedResponse(
            items,
            totalCount,
            totalPages,
            page,
            pageSize,
            query.Filter);

        await cacheService.SetAsync(cacheKey, response, cacheKey.DefaultTtl);

        return Response<GetCommunityFeedResponse>.Success(response);
    }
}

public static class CommunityFeedCacheInvalidator
{
    public static Task InvalidateAsync(ICacheService cacheService, CancellationToken ct = default) =>
        cacheService.RemoveByPattern(CacheKey.Shop.PublicCommunityFeedPattern(), ct);
}
