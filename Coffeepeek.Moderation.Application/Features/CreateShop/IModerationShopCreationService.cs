using Coffeepeek.Moderation.Application.Common.Models;

namespace Coffeepeek.Moderation.Application.Features.CreateShop;

public interface IModerationShopCreationService
{
    Task<Guid> Create(
        SendCoffeeShopToModerationCommand command,
        GeocodingResult? geocodingResult,
        CancellationToken cancellationToken);
}