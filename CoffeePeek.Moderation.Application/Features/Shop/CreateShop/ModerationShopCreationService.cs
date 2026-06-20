using CoffeePeek.Moderation.Application.Common.Models;
using CoffeePeek.Moderation.Domain.Aggregates;
using CoffeePeek.Shared.Kernel;
using DomainPriceRange = CoffeePeek.Moderation.Domain.Aggregates.Enums.PriceRange;
using ModerationShop = CoffeePeek.Moderation.Domain.Aggregates.ModerationShop;

namespace CoffeePeek.Moderation.Application.Features.Shop.CreateShop;

public class ModerationShopCreationService(IModerationShopRepository shopRepository, IUnitOfWork unitOfWork)
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
            command.Description
        );

        var location = geocodingResult != null
            ? new ModerationLocation(command.Address, geocodingResult.Latitude, geocodingResult.Longitude)
            : new ModerationLocation(command.Address);
        shop.SetLocation(location);

        if (command.PriceRange != null)
        {
            shop.AddPriceRange((DomainPriceRange)command.PriceRange.Value);
        }

        if (command.Schedules != null)
        {
            var schedules = command.Schedules.Select(s =>
                (s.DayOfWeek, s.Intervals.Select(i => (i.OpenTime, i.CloseTime)).ToList()));
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
                shop.AddPhoto(photo.FileName, photo.ContentType, photo.StorageKey, photo.Size);
            }
        }

        await shopRepository.AddAsync(shop);

        await unitOfWork.SaveChangesAsync(ct);
        
        return shop.Id;
    }
}
