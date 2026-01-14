using Coffeepeek.Moderation.Application.Common.Models;
using Coffeepeek.Moderation.Application.Features.CreateShop;
using CoffeePeek.Moderation.Domain.Entities;
using CoffeePeek.Moderation.Domain.Repositories;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Abstract.S3;

namespace Coffeepeek.Moderation.Application.Features.Shop.CreateShop;

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
            command.UserId,
            command.CityId,
            command.PriceRange,
            command.Description
        );

        if (geocodingResult != null)
        {
            var location = new ModerationLocation(command.NotValidatedAddress, lat: geocodingResult.Latitude, lon: geocodingResult.Longitude);
            shop.SetLocation(location);
        }

        if (command.Schedules != null)
        {
            var schedules = command.Schedules.Select(s => (s.DayOfWeek, s.Intervals.Select(i => (i.OpenTime, i.CloseTime)).ToList()));
            shop.UpdateSchedules(schedules);
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