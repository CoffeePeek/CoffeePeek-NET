using CoffeePeek.Contract.Dtos.Admin;
using CoffeePeek.Moderation.Domain.Aggregates;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Moderation.Application.Features.Admin.Stats;

public static class GetAdminModerationStatsHandler
{
    public static async Task<Response<AdminServiceStatsDto>> Handle(
        GetAdminModerationStatsQuery _,
        IAdminModerationStatsQueryRepository repository,
        CancellationToken ct)
    {
        var (pendingShops, pendingReviews) = await repository.GetStatsAsync(ct);

        return Response<AdminServiceStatsDto>.Success(new AdminServiceStatsDto(
            PendingModerationShops: pendingShops,
            PendingModerationReviews: pendingReviews));
    }
}
