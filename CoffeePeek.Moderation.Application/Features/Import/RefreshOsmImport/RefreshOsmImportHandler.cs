using CoffeePeek.Contract.Dtos.Import;
using CoffeePeek.Moderation.Application.Abstractions;
using CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Moderation.Application.Features.Import.RefreshOsmImport;

public record RefreshOsmImportCommand;

public static class RefreshOsmImportHandler
{
    public static async Task<Response<OsmRefreshResultDto>> Handle(
        RefreshOsmImportCommand command,
        IOverpassClient overpassClient,
        IShopImportCandidateRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var snapshots = await overpassClient.FetchMinskCafesAsync(ct);
        var now = DateTimeOffset.UtcNow;
        var existing = await repository.GetByExternalIdsAsync(
            ImportSource.Osm,
            snapshots.Select(s => s.ExternalId).ToArray(),
            ct);

        var inserted = 0;
        var updated = 0;

        foreach (var snapshot in snapshots)
        {
            if (existing.TryGetValue(snapshot.ExternalId, out var candidate))
            {
                candidate.RefreshFromOsm(snapshot, now);
                updated++;
            }
            else
            {
                repository.Add(ShopImportCandidate.FromOsm(snapshot, now));
                inserted++;
            }
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Response<OsmRefreshResultDto>.Success(new OsmRefreshResultDto(snapshots.Count, inserted, updated));
    }
}
