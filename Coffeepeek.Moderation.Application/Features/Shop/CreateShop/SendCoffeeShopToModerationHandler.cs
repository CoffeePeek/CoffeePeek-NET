using CoffeePeek.Contract.Abstract;
using CoffeePeek.Moderation.Application.Abstractions;
using CoffeePeek.Moderation.Application.Common.Models;
using CoffeePeek.Moderation.Application.Features.Shop.CreateShop;
using CoffeePeek.Moderation.Domain.Entities;
using CoffeePeek.Moderation.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Moderation.Application.Features.Shop.CreateShop;

public class SendCoffeeShopToModerationHandler(
    IModerationShopRepository repository,
    IModerationShopCreationService creationService,
    IYandexGeocodingService geocodingService,
    ILogger<SendCoffeeShopToModerationHandler> logger)
    : IRequestHandler<SendCoffeeShopToModerationCommand, Response<SendCoffeeShopToModerationResponse>>
{
    public async Task<Response<SendCoffeeShopToModerationResponse>> Handle(
        SendCoffeeShopToModerationCommand command,
        CancellationToken ct)
    {
        logger.LogInformation("Processing moderation request for shop: {ShopName} by user: {UserId}",
            command.Name, command.UserId);

        var duplicateSpec = new DuplicateShopSpecification(command.Name, command.NotValidatedAddress);
        if (await repository.Any(duplicateSpec, ct))
        {
            logger.LogWarning("Duplicate shop detected: {Name} at {Address}",
                command.Name, command.NotValidatedAddress);

            return Response<SendCoffeeShopToModerationResponse>.Error(
                "A coffee shop with that name and address is already located on the edge.");
        }

        var geocodingResult = await TryGeocodeAsync(command.NotValidatedAddress, ct);

        var shopId = await creationService.Create(command, geocodingResult, ct);

        var responseData = new SendCoffeeShopToModerationResponse(shopId, "Pending");

        logger.LogInformation("Shop {ShopId} successfully sent to moderation", shopId);

        return Response<SendCoffeeShopToModerationResponse>.Success(
            responseData,
            "The application has been accepted and will be reviewed by the moderator.");
    }

    private async Task<GeocodingResult?> TryGeocodeAsync(string address, CancellationToken ct)
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