using CoffeePeek.Contract.Dtos.Import;
using CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Moderation.Application.Features.Import.GetImportStats;

public record GetImportStatsQuery;

public static class GetImportStatsHandler
{
    public static async Task<Response<ImportStatsDto>> Handle(
        GetImportStatsQuery query,
        IShopImportCandidateRepository repository,
        CancellationToken ct)
    {
        var stats = await repository.GetStatsAsync(ct);
        return Response<ImportStatsDto>.Success(new ImportStatsDto(
            stats.Pending,
            stats.Skipped,
            stats.Published,
            stats.Rejected,
            stats.Published,
            stats.ByFocus.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value),
            stats.ByBucket.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value)));
    }
}
