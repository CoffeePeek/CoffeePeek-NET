using CoffeePeek.Moderation.Application.Common.Models;
using CoffeePeek.Moderation.Application.Features.Shop.CreateShop;

namespace CoffeePeek.Moderation.Application.Features.Shop.CreateShop;

public interface IModerationShopCreationService
{
    Task<Guid> Create(
        SendCoffeeShopToModerationCommand command,
        GeocodingResult? geocodingResult,
        CancellationToken cancellationToken);
}