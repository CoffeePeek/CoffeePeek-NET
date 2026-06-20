using CoffeePeek.Contract.Dtos.Admin;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Shops.Application.Features.Admin.Stats;

public record GetAdminShopsStatsQuery;

public static class GetAdminShopsStatsHandler
{
    public static async Task<Response<AdminServiceStatsDto>> Handle(
        GetAdminShopsStatsQuery _,
        IAdminStatsQueryRepository repository,
        CancellationToken ct)
    {
        var (totalShops, newShopsToday, totalReviews, newReviewsToday) = await repository.GetStatsAsync(ct);

        return Response<AdminServiceStatsDto>.Success(new AdminServiceStatsDto(
            TotalCoffeeShops: totalShops,
            TotalReviews: totalReviews,
            NewCoffeeShopsToday: newShopsToday,
            NewReviewsToday: newReviewsToday));
    }
}
