using Coffeepeek.Moderation.Application.Commands;

namespace CoffeePeek.ModerationService.Services.Interfaces;

public interface IModerationShopCreationService
{
    Task<Guid> CreateAsync(
        SendCoffeeShopToModerationRequest request,
        GeocodingResult? geocodingResult,
        CancellationToken cancellationToken);
}