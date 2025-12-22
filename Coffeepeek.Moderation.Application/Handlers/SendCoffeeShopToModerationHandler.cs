using CoffeePeek.Contract.Response.CoffeeShop.Review;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop.Review;
using CoffeePeek.Moderation.Application.Commands;
using Coffeepeek.Moderation.Application.Services;
using CoffeePeek.Moderation.Domain.Repositories;
using CoffeePeek.ModerationService.Services.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Moderation.Application.Handlers;

public class SendCoffeeShopToModerationHandler(
    IModerationShopRepository repository,
    IModerationShopCreationService creationService,
    IYandexGeocodingService geocodingService,
    ILogger<SendCoffeeShopToModerationHandler> logger)
    : IRequestHandler<SendCoffeeShopToModerationCommand, Response<SendCoffeeShopToModerationResponse>>
{
    public async Task<Response<SendCoffeeShopToModerationResponse>> Handle(SendCoffeeShopToModerationCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Attempting to send coffee shop '{CoffeeShopName}' to moderation for user '{UserId}'.",
            command.Name,
            command.UserId);

        var existingShop = await repository.GetByNameAndAddressAsync(
            command.Name,
            command.NotValidatedAddress,
            command.UserId);

        if (existingShop != null)
        {
            logger.LogWarning(
                "Moderation submission for coffee shop '{CoffeeShopName}' by user '{UserId}' already exists.",
                command.Name,
                command.UserId);

            return Response<SendCoffeeShopToModerationResponse>.Error(
                "A moderation submission with this name and address already exists.");
        }

        var geocodingResult = await TryGeocodeAsync(command.NotValidatedAddress, cancellationToken);

        var shopId = await creationService.CreateAsync(command, geocodingResult, cancellationToken);

        logger.LogInformation(
            "ModerationShop '{ModerationShopId}' successfully submitted for moderation.",
            shopId);

        return Response<SendCoffeeShopToModerationResponse>.Success(
            null!,
            "CoffeeShop added to moderation.");
    }

    private async Task<GeocodingResult?> TryGeocodeAsync(string address, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(address))
            return null;

        try
        {
            return await geocodingService.GeocodeAsync(address, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Geocoding failed for address '{Address}'. Continuing without coordinates.", address);
            return null;
        }
    }
}