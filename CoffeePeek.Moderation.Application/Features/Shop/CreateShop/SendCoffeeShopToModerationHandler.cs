using CoffeePeek.Moderation.Application.Abstractions;
using CoffeePeek.Moderation.Application.Common.Models;
using CoffeePeek.Moderation.Application.ErrorCodes;
using CoffeePeek.Moderation.Domain.Aggregates;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using Microsoft.Extensions.Logging;
using Wolverine.Attributes;

namespace CoffeePeek.Moderation.Application.Features.Shop.CreateShop;

public static class SendCoffeeShopToModerationHandler
{
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

            throw new ConflictException(
                "A coffee shop with that name and address is already pending moderation or has been approved.",
                ShopModerationErrorCodes.DuplicateShop);
        }

        var geocodingResult = await TryGeocodeAsync(command.Address, geocodingService, logger, ct);
        var isAddressValidated = geocodingResult != null;

        if (!isAddressValidated)
        {
            logger.LogWarning(
                "Geocoding failed for address: {Address}. Shop will be saved with unvalidated address.",
                command.Address);
        }

        var shopId = await creationService.Create(command, geocodingResult, ct);

        var responseData = new SendCoffeeShopToModerationResponse(shopId, "Pending", isAddressValidated);

        logger.LogInformation("Shop {ShopId} successfully sent to moderation", shopId);

        var message = isAddressValidated
            ? "The application has been accepted and will be reviewed by the moderator."
            : "The application has been accepted. Address coordinates could not be verified automatically and will be checked by a moderator.";

        return Response<SendCoffeeShopToModerationResponse>.Success(responseData, message);
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
