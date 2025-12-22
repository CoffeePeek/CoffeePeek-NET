using CoffeePeek.Moderation.Application.Commands;
using Coffeepeek.Moderation.Application.Services;
using CoffeePeek.Moderation.Application.Services;
using CoffeePeek.Moderation.Domain;
using CoffeePeek.Moderation.Domain.Entities;
using CoffeePeek.Moderation.Domain.Repositories;
using CoffeePeek.ModerationService.Services.Interfaces;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MapsterMapper;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Moderation.Infrastructure.Services;

public class ModerationShopCreationService(
    ModerationDbContext dbContext,
    IModerationShopRepository shopRepository,
    IModerationScheduleService scheduleService,
    IModerationRelationsService relationsService,
    IGenericRepository<PhotoMetadata> photoRepository,
    IStorageService storageService,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<ModerationShopCreationService> logger)
    : IModerationShopCreationService
{
    public async Task<Guid> CreateAsync(
        SendCoffeeShopToModerationCommand command,
        GeocodingResult? geocodingResult,
        CancellationToken cancellationToken)
    {
        var moderationShop = mapper.Map<ModerationShop>(command);

        // Always persist the not validated address as the main address so it is never null,
        // even if geocoding fails.
        moderationShop.Address = command.NotValidatedAddress;

        if (geocodingResult != null)
        {
            moderationShop.IsAddressValidated = true;
            moderationShop.Latitude = geocodingResult.Latitude;
            moderationShop.Longitude = geocodingResult.Longitude;
        }

        AddShopContactsIfNeeded(command, moderationShop);
        AddLocationIfNeeded(command, moderationShop, geocodingResult);

        await shopRepository.AddAsync(moderationShop);

        await scheduleService.AddSchedulesAsync(
            moderationShop.Id,
            command.Schedules,
            cancellationToken);

        await relationsService.AddEquipmentsAsync(
            moderationShop.Id,
            command.EquipmentIds,
            cancellationToken);

        await relationsService.AddCoffeeBeansAsync(
            moderationShop.Id,
            command.CoffeeBeanIds,
            cancellationToken);

        await relationsService.AddRoastersAsync(
            moderationShop.Id,
            command.RoasterIds,
            cancellationToken);

        await relationsService.AddBrewMethodsAsync(
            moderationShop.Id,
            command.BrewMethodIds,
            cancellationToken);

        foreach (var photoDto in command.ShopPhotos)
        {
            using var stream = new MemoryStream(photoDto.Data);
        
            var storageKey = await storageService.UploadFileAsync(
                stream, 
                photoDto.ContentType, 
                cancellationToken);
            
            var photo = new PhotoMetadata(
                photoDto.FileName,
                photoDto.ContentType,
                storageKey,
                photoDto.Data.Length,
                command.UserId,
                moderationShop.Id);

            await photoRepository.AddAsync(photo, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "ModerationShop '{ModerationShopId}' successfully created for user '{UserId}'.",
            moderationShop.Id,
            command.UserId);

        return moderationShop.Id;
    }

    private void AddShopContactsIfNeeded(
        SendCoffeeShopToModerationCommand command,
        ModerationShop moderationShop)
    {
        if (command.ShopContact == null)
            return;

        var shopContact = new ShopContacts
        {
            Id = Guid.NewGuid(),
            PhoneNumber = command.ShopContact.PhoneNumber,
            InstagramLink = command.ShopContact.InstagramLink,
            Email = command.ShopContact.Email,
            SiteLink = command.ShopContact.SiteLink
        };

        dbContext.ShopContacts.Add(shopContact);
        moderationShop.ShopContactId = shopContact.Id;
    }

    private void AddLocationIfNeeded(
        SendCoffeeShopToModerationCommand command,
        ModerationShop moderationShop,
        GeocodingResult? geocodingResult)
    {
        if (geocodingResult is null)
            return;

        var location = new Location
        {
            Id = Guid.NewGuid(),
            ShopId = moderationShop.Id,
            Address = command.NotValidatedAddress,
            Latitude = geocodingResult.Latitude,
            Longitude = geocodingResult.Longitude
        };

        dbContext.ModerationLocations.Add(location);
        moderationShop.LocationId = location.Id;
        moderationShop.Address = command.NotValidatedAddress;
    }
}