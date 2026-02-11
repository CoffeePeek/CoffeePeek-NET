using CoffeePeek.Moderation.Application.Abstractions;
using CoffeePeek.Moderation.Application.Common.Models;
using CoffeePeek.Moderation.Domain.Aggregates;
using CoffeePeek.Shared.Kernel.Response;
using Microsoft.Extensions.Logging;
using Wolverine.Attributes;

namespace CoffeePeek.Moderation.Application.Features.Shop.CreateShop;

public static class SendCoffeeShopToModerationHandler
{
    [Transactional]
    public static async Task<Response<SendCoffeeShopToModerationResponse>> Handle(
        SendCoffeeShopToModerationCommand command,
        IModerationShopRepository repository,
        IModerationShopCreationService creationService,
        IYandexGeocodingService geocodingService,
        ILogger<SendCoffeeShopToModerationCommand> logger,
        CancellationToken ct)
    {
        logger.LogInformation("Processing moderation request for shop: {ShopName} by user: {UserId}",
            command.Name, command.UserId);

        var duplicateSpec = new DuplicateShopSpecification(command.Name, command.Address);
        if (await repository.Any(duplicateSpec, ct))
        {
            logger.LogWarning("Duplicate shop detected: {Name} at {Address}",
                command.Name, command.Address);

            return Response<SendCoffeeShopToModerationResponse>.Error(
                "A coffee shop with that name and address is already located on the edge.");
        }

        var geocodingResult = await TryGeocodeAsync(command.Address, geocodingService, logger, ct);

        var shopId = await creationService.Create(command, geocodingResult, ct);

        var responseData = new SendCoffeeShopToModerationResponse(shopId, "Pending");

        logger.LogInformation("Shop {ShopId} successfully sent to moderation", shopId);

        return Response<SendCoffeeShopToModerationResponse>.Success(
            responseData,
            "The application has been accepted and will be reviewed by the moderator.");
    }

    private static async Task<GeocodingResult?> TryGeocodeAsync(
        string address,
        IYandexGeocodingService geocodingService,
        ILogger logger,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(address)) return null;

        try
        {
            return await geocodingService.GeocodeAsync(address, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Geocoding service unavailable for address: {Address}", address);
            return null;
        }
    }
}
