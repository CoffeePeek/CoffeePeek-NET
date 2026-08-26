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
        ICoffeeMapCatalog coffeeMapCatalog,
        IShopImportCandidateRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        IReadOnlyList<OsmCandidateSnapshot> snapshots;
        try
        {
            snapshots = await overpassClient.FetchMinskCafesAsync(ct);
        }
        catch (Exception ex) when (ex is InvalidOperationException or HttpRequestException or TaskCanceledException)
        {
            return Response<OsmRefreshResultDto>.Error(
                System.Net.HttpStatusCode.GatewayTimeout,
                $"OSM Overpass refresh failed: {ex.Message}");
        }
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

        var coffeeMap = await coffeeMapCatalog.GetCafesAsync(ct);
        var existingCoffeeMap = await repository.GetByExternalIdsAsync(
            ImportSource.CoffeeMap,
            coffeeMap.Select(s => s.ExternalId).ToArray(),
            ct);
        var coffeeMapInserted = 0;
        var coffeeMapUpdated = 0;
        foreach (var snapshot in coffeeMap)
        {
            if (existingCoffeeMap.TryGetValue(snapshot.ExternalId, out var candidate))
            {
                candidate.RefreshFromCoffeeMap(snapshot, now);
                coffeeMapUpdated++;
            }
            else
            {
                repository.Add(ShopImportCandidate.FromCoffeeMap(snapshot, now));
                coffeeMapInserted++;
            }
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Response<OsmRefreshResultDto>.Success(new OsmRefreshResultDto(
            snapshots.Count,
            inserted,
            updated,
            coffeeMap.Count,
            coffeeMapInserted,
            coffeeMapUpdated));
    }
}
