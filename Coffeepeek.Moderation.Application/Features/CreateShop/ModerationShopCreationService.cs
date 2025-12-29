using CoffeePeek.Contract.Constants;
using Coffeepeek.Moderation.Application.Abstractions;
using Coffeepeek.Moderation.Application.Common.Models;
using Coffeepeek.Moderation.Application.Features.CreateShop;
using CoffeePeek.Moderation.Domain.Entities;
using CoffeePeek.Moderation.Domain.Repositories;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Abstract.S3;

namespace CoffeePeek.Moderation.Application.Features.CreateShop;

public class ModerationShopCreationService(
    IModerationShopRepository shopRepository,
    IStorageService storageService,
    IUnitOfWork unitOfWork)
    : IModerationShopCreationService
{
    public async Task<Guid> Create(
        SendCoffeeShopToModerationCommand command,
        GeocodingResult? geocodingResult,
        CancellationToken ct)
    {
        var shop = ModerationShop.Create(
            command.Name,
            command.NotValidatedAddress,
            command.UserId,
            command.CityId ?? BusinessConstants.DefaultUnAuthorizedCityId,
            command.PriceRange,
            command.Description
        );

        if (geocodingResult != null)
        {
            shop.SetLocation(geocodingResult.Latitude, geocodingResult.Longitude, command.NotValidatedAddress);
        }

        if (command.Schedules != null)
        {
            shop.UpdateSchedules(command.Schedules);
        }
        
        shop.UpdateRelations(
            command.EquipmentIds ?? [], 
            command.CoffeeBeanIds ?? [],
            command.RoasterIds ?? [],
                command.BrewMethodIds ?? []);

        if (command.ShopPhotos != null)
        {
            foreach (var photo in command.ShopPhotos)
            {
                await storageService.MarkAsPermanentAsync(photo.StorageKey);
            
                shop.AddPhoto(photo.FileName, photo.ContentType, photo.StorageKey, photo.Size);
            }
        }

        await shopRepository.AddAsync(shop);
        await unitOfWork.SaveChangesAsync(ct);

        return shop.Id;
    }
}