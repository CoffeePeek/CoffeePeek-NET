using CoffeePeek.Moderation.Application.Common.Models;

namespace CoffeePeek.Moderation.Application.Features.Shop.CreateShop;

public interface IModerationShopCreationService
{
    Task<Guid> Create(
        SendCoffeeShopToModerationCommand command,
        GeocodingResult? geocodingResult,
        CancellationToken cancellationToken);
}