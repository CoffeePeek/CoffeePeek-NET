using CoffeePeek.Contract.Events.Menu;
using CoffeePeek.Moderation.Application.Features.Menu;
using CoffeePeek.Moderation.Domain.Aggregates;
using CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;
using CoffeePeek.Shared.Kernel;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Moderation.Infrastructure.Consumers;

public class MenuParsedEventHandler(
    IShopImportCandidateRepository candidateRepository,
    IModerationShopRepository moderationShopRepository,
    IUnitOfWork unitOfWork,
    ILogger<MenuParsedEventHandler> logger)
{
    public async Task Handle(MenuParsedEvent message, CancellationToken ct)
    {
        try
        {
            var items = MenuDraftMapper.ToDraftItems(message.Items);
            var unmatched = MenuDraftMapper.ToDraftUnmatched(message.Unmatched);
            var suggested = message.SuggestedPriceRange is null ? (int?)null : (int)message.SuggestedPriceRange.Value;

            switch (message.SourceKind)
            {
                case MenuParseSourceKind.ImportCandidate:
                {
                    var candidate = await candidateRepository.GetByIdAsync(message.SourceId, ct);
                    if (candidate is null)
                    {
                        logger.LogWarning(
                            "Menu parsed event skipped: import candidate {SourceId} was not found (success={Success})",
                            message.SourceId,
                            message.Success);
                        return;
                    }

                    candidate.ApplyMenuParseResult(
                        message.Success, message.Error, suggested, items, unmatched, message.CapturedAtUtc);
                    break;
                }
                case MenuParseSourceKind.ModerationShop:
                {
                    var shop = await moderationShopRepository.GetByIdAsync(message.SourceId, ct);
                    if (shop is null)
                    {
                        logger.LogWarning(
                            "Menu parsed event skipped: moderation shop {SourceId} was not found (success={Success})",
                            message.SourceId,
                            message.Success);
                        return;
                    }

                    shop.ApplyMenuParseResult(
                        message.Success, message.Error, suggested, items, unmatched, message.CapturedAtUtc);
                    break;
                }
                default:
                    logger.LogInformation(
                        "Menu parsed event ignored for published shop {SourceId}",
                        message.SourceId);
                    return;
            }

            await unitOfWork.SaveChangesAsync(ct);
            logger.LogInformation(
                "Applied menu parse result to {Kind} {SourceId} success={Success} items={ItemCount} unmatched={UnmatchedCount}",
                message.SourceKind,
                message.SourceId,
                message.Success,
                message.Items.Count,
                message.Unmatched.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to apply menu parse result to {Kind} {SourceId}",
                message.SourceKind,
                message.SourceId);
            throw;
        }
    }
}
