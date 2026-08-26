using CoffeePeek.Contract.Events.Moderation;

namespace CoffeePeek.Shops.Application.Services;

public interface IEnrichShopFromImportService
{
    Task<int> EnrichShopsFromImportAsync(IReadOnlyList<ImportShopEnrichmentItem> items, CancellationToken ct = default);
}
