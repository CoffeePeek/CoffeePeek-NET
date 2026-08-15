using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Dtos.Admin;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Account.Application.Features.Admin.Stats;

public static class GetAdminOverviewStatsHandler
{
    public static async Task<Response<AdminOverviewStatsDto>> Handle(
        GetAdminOverviewStatsQuery _,
        IAdminUserQueryRepository userRepository,
        IAdminStatsClient statsClient,
        CancellationToken ct)
    {
        var userStats = await userRepository.GetStatsAsync(ct);
        var platform = await statsClient.GetPlatformStatsAsync(ct);

        return Response<AdminOverviewStatsDto>.Success(new AdminOverviewStatsDto(
            TotalUsers: userStats.TotalUsers,
            UsersRegisteredToday: userStats.RegisteredToday,
            ActiveUsers: userStats.ActiveUsers,
            BlockedUsers: userStats.BlockedUsers,
            TotalCoffeeShops: platform.TotalCoffeeShops,
            TotalReviews: platform.TotalReviews,
            PendingModerationShops: platform.PendingModerationShops,
            PendingModerationReviews: platform.PendingModerationReviews,
            NewCoffeeShopsToday: platform.NewCoffeeShopsToday,
            NewReviewsToday: platform.NewReviewsToday,
            Import: new AdminImportOverviewStatsDto(
                platform.ImportPending,
                platform.ImportPublished,
                platform.ImportRejected,
                platform.ImportSkipped,
                platform.ImportPublished),
            ShopsAvailable: platform.ShopsAvailable,
            ModerationAvailable: platform.ModerationAvailable));
    }
}
