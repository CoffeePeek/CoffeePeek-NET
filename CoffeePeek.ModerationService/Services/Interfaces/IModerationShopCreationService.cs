using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.ModerationService.Services.Interfaces;

namespace CoffeePeek.ModerationService.Services.Interfaces;

public interface IModerationShopCreationService
{
    Task<Guid> CreateAsync(
        SendCoffeeShopToModerationRequest request,
        GeocodingResult? geocodingResult,
        CancellationToken cancellationToken);
}




