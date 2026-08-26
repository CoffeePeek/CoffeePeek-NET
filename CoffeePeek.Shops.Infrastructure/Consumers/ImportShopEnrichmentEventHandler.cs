using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Shops.Application.Services;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Shops.Infrastructure.Consumers;

public class ImportShopEnrichmentEventHandler(
    IEnrichShopFromImportService enrichShopService,
    ILogger<ImportShopEnrichmentEventHandler> logger)
{
    public async Task Handle(ImportShopEnrichmentEvent message, CancellationToken ct)
    {
        var enriched = await enrichShopService.EnrichShopsFromImportAsync(message.Items, ct);
        logger.LogInformation(
            "Import dump enrichment applied to {EnrichedCount} of {ItemCount} published shops",
            enriched,
            message.Items.Count);
    }
}
