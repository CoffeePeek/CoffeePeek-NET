using CoffeePeek.Moderation.Application.Commands;
using CoffeePeek.ModerationService.Services.Interfaces;

namespace Coffeepeek.Moderation.Application.Services;

public interface IModerationShopCreationService
{
    Task<Guid> CreateAsync(
        SendCoffeeShopToModerationCommand command,
        GeocodingResult? geocodingResult,
        CancellationToken cancellationToken);
}