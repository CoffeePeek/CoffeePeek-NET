using CoffeePeek.Contract.Dtos.Public;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;

namespace CoffeePeek.Shops.Application.Features.Public.Stats;

public static class GetPublicStatsHandler
{
    public static async Task<Response<PublicPlatformStatsDto>> Handle(
        GetPublicStatsQuery _,
        IPublicStatsQueryRepository repository,
        ICacheService cacheService,
        CancellationToken ct)
    {
        var cacheKey = CacheKey.Shop.PublicPlatformStats();
        var cached = await cacheService.GetAsync<PublicPlatformStatsDto>(cacheKey, ct);

        if (cached is not null)
            return Response<PublicPlatformStatsDto>.Success(cached);

        var stats = await repository.GetStatsAsync(ct);
        var dto = new PublicPlatformStatsDto(
            TotalCoffeeShops: stats.TotalCoffeeShops,
            TotalReviews: stats.TotalReviews,
            TotalCheckIns: stats.TotalCheckIns,
            AverageRating: stats.AverageRating);

        await cacheService.SetAsync(cacheKey, dto);

        return Response<PublicPlatformStatsDto>.Success(dto);
    }
}

public static class PublicStatsCacheInvalidator
{
    public static Task InvalidateAsync(ICacheService cacheService, CancellationToken ct = default) =>
        cacheService.RemoveAsync(CacheKey.Shop.PublicPlatformStats());
}
