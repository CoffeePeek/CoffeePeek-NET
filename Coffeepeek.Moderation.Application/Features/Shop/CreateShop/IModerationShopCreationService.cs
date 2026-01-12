using Coffeepeek.Moderation.Application.Common.Models;
using Coffeepeek.Moderation.Application.Features.Shop.CreateShop;

namespace Coffeepeek.Moderation.Application.Features.CreateShop;

public interface IModerationShopCreationService
{
    Task<Guid> Create(
        SendCoffeeShopToModerationCommand command,
        GeocodingResult? geocodingResult,
        CancellationToken cancellationToken);
}