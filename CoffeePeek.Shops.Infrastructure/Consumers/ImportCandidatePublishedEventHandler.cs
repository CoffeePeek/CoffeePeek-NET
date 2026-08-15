using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shops.Application.Services;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Shops.Infrastructure.Consumers;

public class ImportCandidatePublishedEventHandler(
    ICreateShopFromImportService createShopService,
    ILogger<ImportCandidatePublishedEventHandler> logger)
{
    public async Task<ImportCandidatePublishCompleteResponse> Handle(
        ImportCandidatePublishedEvent message,
        CancellationToken ct)
    {
        var results = new List<ImportCandidatePublishResult>(message.Items.Count);

        foreach (var item in message.Items)
        {
            logger.LogInformation(
                "Creating shop from import candidate {CandidateId} ({ShopName})",
                item.CandidateId,
                item.Name);

            var shopId = await createShopService.CreateShopFromImportAsync(item, ct);
            results.Add(new ImportCandidatePublishResult(item.CandidateId, shopId));
        }

        return new ImportCandidatePublishCompleteResponse(results);
    }
}
