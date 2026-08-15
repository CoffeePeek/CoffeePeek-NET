using CoffeePeek.Contract.Dtos.Admin;
using CoffeePeek.Moderation.Domain.Aggregates;
using CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Moderation.Application.Features.Admin.Stats;

public static class GetAdminModerationStatsHandler
{
    public static async Task<Response<AdminServiceStatsDto>> Handle(
        GetAdminModerationStatsQuery _,
        IAdminModerationStatsQueryRepository repository,
        IShopImportCandidateRepository importRepository,
        CancellationToken ct)
    {
        var pendingTask = repository.GetStatsAsync(ct);
        var importTask = importRepository.GetStatsAsync(ct);
        await Task.WhenAll(pendingTask, importTask);

        var (pendingShops, pendingReviews) = await pendingTask;
        var import = await importTask;

        return Response<AdminServiceStatsDto>.Success(new AdminServiceStatsDto(
            PendingModerationShops: pendingShops,
            PendingModerationReviews: pendingReviews,
            ImportPending: import.Pending,
            ImportPublished: import.Published,
            ImportRejected: import.Rejected,
            ImportSkipped: import.Skipped));
    }
}
