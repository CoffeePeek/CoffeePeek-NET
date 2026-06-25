using CoffeePeek.Contract.Dtos.Public;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Aggregates.CommunityFollowAggregate;

namespace CoffeePeek.Shops.Application.Features.Public.Feed;

public static class GetCommunityFeedHandler
{
    public static async Task<Response<GetCommunityFeedResponse>> Handle(
        GetCommunityFeedQuery query,
        ICommunityFeedQueries repository,
        IQueryCommunityUserFollowRepository followRepository,
        IQueryCommunityCityFollowRepository cityFollowRepository,
        IQueryCoffeeShopRepository coffeeShopRepository,
        ICacheService cacheService,
        CancellationToken ct)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);

        IReadOnlyList<Guid>? followingUserIds = null;
        HashSet<Guid>? cityShopIds = null;

        if (query.Filter == CommunityFeedFilter.Following)
        {
            if (query.ViewerUserId is not { } viewerId)
                throw new UnauthorizedException("Authentication is required for the Following feed.");

            followingUserIds = await followRepository.GetFollowingUserIdsAsync(viewerId, ct);
            if (followingUserIds.Count == 0)
            {
                return Response<GetCommunityFeedResponse>.Success(new GetCommunityFeedResponse(
                    [], 0, 0, page, pageSize, query.Filter, query.CityId));
            }
        }
        else if (query.Filter == CommunityFeedFilter.FollowedCities)
        {
            if (query.ViewerUserId is not { } viewerId)
                throw new UnauthorizedException("Authentication is required for the FollowedCities feed.");

            var followedCityIds = await cityFollowRepository.GetFollowedCityIdsAsync(viewerId, ct);
            if (followedCityIds.Count == 0)
            {
                return Response<GetCommunityFeedResponse>.Success(new GetCommunityFeedResponse(
                    [], 0, 0, page, pageSize, query.Filter, query.CityId));
            }

            cityShopIds = await coffeeShopRepository.GetShopIdsByCityIdsAsync(followedCityIds, ct);
            if (cityShopIds.Count == 0)
            {
                return Response<GetCommunityFeedResponse>.Success(new GetCommunityFeedResponse(
                    [], 0, 0, page, pageSize, query.Filter, query.CityId));
            }
        }

        var context = new CommunityFeedQueryContext(query.CityId, query.ViewerUserId, followingUserIds, cityShopIds);
        var cacheKey = CacheKey.Shop.PublicCommunityFeed(
            page, pageSize, query.Filter.ToString(), query.CityId, query.ViewerUserId);

        if (query.ViewerUserId is null)
        {
            var cached = await cacheService.GetAsync<GetCommunityFeedResponse>(cacheKey, ct);
            if (cached is not null)
                return Response<GetCommunityFeedResponse>.Success(cached);
        }

        var (items, totalCount) = await repository.GetFeedAsync(page, pageSize, query.Filter, context, ct);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        var response = new GetCommunityFeedResponse(
            items,
            totalCount,
            totalPages,
            page,
            pageSize,
            query.Filter,
            query.CityId);

        if (query.ViewerUserId is null)
            await cacheService.SetAsync(cacheKey, response, cacheKey.DefaultTtl);

        return Response<GetCommunityFeedResponse>.Success(response);
    }
}

public static class CommunityFeedCacheInvalidator
{
    public static Task InvalidateAsync(ICacheService cacheService, CancellationToken ct = default) =>
        cacheService.RemoveByPattern(CacheKey.Shop.PublicCommunityFeedPattern(), ct);
}
