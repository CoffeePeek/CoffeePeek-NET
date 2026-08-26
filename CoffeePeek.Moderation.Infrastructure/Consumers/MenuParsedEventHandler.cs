using CoffeePeek.Contract.Events.Menu;
using CoffeePeek.Moderation.Application.Features.Menu;
using CoffeePeek.Moderation.Domain.Aggregates;
using CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;
using CoffeePeek.Shared.Kernel;

namespace CoffeePeek.Moderation.Infrastructure.Consumers;

public class MenuParsedEventHandler(
    IShopImportCandidateRepository candidateRepository,
    IModerationShopRepository moderationShopRepository,
    IUnitOfWork unitOfWork)
{
    public async Task Handle(MenuParsedEvent message, CancellationToken ct)
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
                    return;
                candidate.ApplyMenuParseResult(
                    message.Success, message.Error, suggested, items, unmatched, message.CapturedAtUtc);
                break;
            }
            case MenuParseSourceKind.ModerationShop:
            {
                var shop = await moderationShopRepository.GetByIdAsync(message.SourceId, ct);
                if (shop is null)
                    return;
                shop.ApplyMenuParseResult(
                    message.Success, message.Error, suggested, items, unmatched, message.CapturedAtUtc);
                break;
            }
            default:
                return;
        }

        await unitOfWork.SaveChangesAsync(ct);
    }
}
