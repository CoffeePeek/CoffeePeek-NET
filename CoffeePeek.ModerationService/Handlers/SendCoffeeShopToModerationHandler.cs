using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Contract.Response.CoffeeShop.Review;
using CoffeePeek.Contract.Responses;
using CoffeePeek.ModerationService.Repositories.Interfaces;
using CoffeePeek.ModerationService.Services.Interfaces;
using MediatR;

namespace CoffeePeek.ModerationService.Handlers;

public class SendCoffeeShopToModerationHandler(
    IModerationShopRepository repository,
    IModerationShopCreationService creationService,
    IYandexGeocodingService geocodingService,
    ILogger<SendCoffeeShopToModerationHandler> logger)
    : IRequestHandler<SendCoffeeShopToModerationRequest, Response<SendCoffeeShopToModerationResponse>>
{
    public async Task<Response<SendCoffeeShopToModerationResponse>> Handle(SendCoffeeShopToModerationRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Attempting to send coffee shop '{CoffeeShopName}' to moderation for user '{UserId}'.",
            request.Name,
            request.UserId);

        var existingShop = await repository.GetByNameAndAddressAsync(
            request.Name,
            request.NotValidatedAddress,
            request.UserId);

        if (existingShop != null)
        {
            logger.LogWarning(
                "Moderation submission for coffee shop '{CoffeeShopName}' by user '{UserId}' already exists.",
                request.Name,
                request.UserId);

            return Response<SendCoffeeShopToModerationResponse>.Error(
                "A moderation submission with this name and address already exists.");
        }

        var geocodingResult = await TryGeocodeAsync(request.NotValidatedAddress, cancellationToken);

        var shopId = await creationService.CreateAsync(request, geocodingResult, cancellationToken);

        logger.LogInformation(
            "ModerationShop '{ModerationShopId}' successfully submitted for moderation.",
            shopId);

        return Response<SendCoffeeShopToModerationResponse>.Success(
            null,
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