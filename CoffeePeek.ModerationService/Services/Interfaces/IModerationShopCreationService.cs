using CoffeePeek.Contract.Requests.CoffeeShop.Review;

namespace CoffeePeek.ModerationService.Services.Interfaces;

public interface IModerationShopCreationService
{
    Task<Guid> CreateAsync(
        SendCoffeeShopToModerationRequest request,
        GeocodingResult? geocodingResult,
        CancellationToken cancellationToken);
}