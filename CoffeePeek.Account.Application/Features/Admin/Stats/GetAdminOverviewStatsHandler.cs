using CoffeePeek.Account.Application.Features.Admin.Stats;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Dtos.Admin;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Account.Application.Features.Admin.Stats;

public record GetAdminOverviewStatsQuery;

public static class GetAdminOverviewStatsHandler
{
    public static async Task<Response<AdminOverviewStatsDto>> Handle(
        GetAdminOverviewStatsQuery _,
        IAdminUserQueryRepository userRepository,
        IAdminStatsClient statsClient,
        CancellationToken ct)
    {
        var userStats = await userRepository.GetStatsAsync(ct);
        var platformStats = await statsClient.GetPlatformStatsAsync(ct);

        return Response<AdminOverviewStatsDto>.Success(new AdminOverviewStatsDto(
            TotalUsers: userStats.TotalUsers,
            UsersRegisteredToday: userStats.RegisteredToday,
            TotalCoffeeShops: platformStats.TotalCoffeeShops,
            TotalReviews: platformStats.TotalReviews,
            PendingModerationShops: platformStats.PendingModerationShops,
            PendingModerationReviews: platformStats.PendingModerationReviews,
            NewCoffeeShopsToday: platformStats.NewCoffeeShopsToday,
            NewReviewsToday: platformStats.NewReviewsToday));
    }
}
