using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.ModerationService.Configuration;
using CoffeePeek.ModerationService.Entities;
using CoffeePeek.ModerationService.Models;
using CoffeePeek.ModerationService.Repositories.Interfaces;
using CoffeePeek.ModerationService.Services.Interfaces;
using MapsterMapper;

namespace CoffeePeek.ModerationService.Services;

public class ModerationShopCreationService(
    ModerationDbContext dbContext,
    IModerationShopRepository shopRepository,
    IModerationScheduleService scheduleService,
    IModerationRelationsService relationsService,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<ModerationShopCreationService> logger)
    : IModerationShopCreationService
{
    public async Task<Guid> CreateAsync(
        SendCoffeeShopToModerationRequest request,
        GeocodingResult? geocodingResult,
        CancellationToken cancellationToken)
    {
        var moderationShop = mapper.Map<ModerationShop>(request);

        if (geocodingResult != null)
        {
            moderationShop.IsAddressValidated = true;
            moderationShop.Latitude = geocodingResult.Latitude;
            moderationShop.Longitude = geocodingResult.Longitude;
        }

        AddShopContactsIfNeeded(request, moderationShop);
        AddLocationIfNeeded(request, moderationShop, geocodingResult);

        await shopRepository.AddAsync(moderationShop);

        await scheduleService.AddSchedulesAsync(
            moderationShop.Id,
            request.Schedules,
            cancellationToken);

        await relationsService.AddEquipmentsAsync(
            moderationShop.Id,
            request.EquipmentIds,
            cancellationToken);

        await relationsService.AddCoffeeBeansAsync(
            moderationShop.Id,
            request.CoffeeBeanIds,
            cancellationToken);

        await relationsService.AddRoastersAsync(
            moderationShop.Id,
            request.RoasterIds,
            cancellationToken);

        await relationsService.AddBrewMethodsAsync(
            moderationShop.Id,
            request.BrewMethodIds,
            cancellationToken);

        // TODO: обработка ShopPhotos после внедрения файлового стораджа

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "ModerationShop '{ModerationShopId}' successfully created for user '{UserId}'.",
            moderationShop.Id,
            request.UserId);

        return moderationShop.Id;
    }

    private void AddShopContactsIfNeeded(
        SendCoffeeShopToModerationRequest request,
        ModerationShop moderationShop)
    {
        if (request.ShopContact == null)
            return;

        var shopContact = new ShopContacts
        {
            Id = Guid.NewGuid(),
            PhoneNumber = request.ShopContact.PhoneNumber,
            InstagramLink = request.ShopContact.InstagramLink,
            Email = request.ShopContact.Email,
            SiteLink = request.ShopContact.SiteLink
        };

        dbContext.ShopContacts.Add(shopContact);
        moderationShop.ShopContactId = shopContact.Id;
    }

    private void AddLocationIfNeeded(
        SendCoffeeShopToModerationRequest request,
        ModerationShop moderationShop,
        GeocodingResult? geocodingResult)
    {
        if (geocodingResult is null)
            return;

        var location = new Location
        {
            Id = Guid.NewGuid(),
            ShopId = moderationShop.Id,
            Address = request.NotValidatedAddress,
            Latitude = geocodingResult.Latitude,
            Longitude = geocodingResult.Longitude
        };

        dbContext.ModerationLocations.Add(location);
        moderationShop.LocationId = location.Id;
        moderationShop.Address = request.NotValidatedAddress;
    }
}




