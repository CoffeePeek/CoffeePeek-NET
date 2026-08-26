using CoffeePeek.Contract.Dtos.Import;
using CoffeePeek.Moderation.Application.Abstractions;
using CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Moderation.Application.Features.Import.RefreshCoffeeMapImport;

public record RefreshCoffeeMapImportCommand;

public static class RefreshCoffeeMapImportHandler
{
    public static async Task<Response<CoffeeMapRefreshResultDto>> Handle(
        RefreshCoffeeMapImportCommand command,
        ICoffeeMapCatalog catalog,
        IShopImportCandidateRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var snapshots = await catalog.GetCafesAsync(ct);
        var now = DateTimeOffset.UtcNow;
        var existing = await repository.GetByExternalIdsAsync(
            ImportSource.CoffeeMap,
            snapshots.Select(s => s.ExternalId).ToArray(),
            ct);

        var inserted = 0;
        var updated = 0;

        foreach (var snapshot in snapshots)
        {
            if (existing.TryGetValue(snapshot.ExternalId, out var candidate))
            {
                candidate.RefreshFromCoffeeMap(snapshot, now);
                updated++;
            }
            else
            {
                repository.Add(ShopImportCandidate.FromCoffeeMap(snapshot, now));
                inserted++;
            }
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Response<CoffeeMapRefreshResultDto>.Success(
            new CoffeeMapRefreshResultDto(snapshots.Count, inserted, updated));
    }
}
