using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Events;
using CoffeePeek.Moderation.Application.Abstractions;
using CoffeePeek.Moderation.Domain.Aggregates;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Response;
using MapsterMapper;
using Wolverine;
using DomainPriceRange = CoffeePeek.Moderation.Domain.Aggregates.Enums.PriceRange;

namespace CoffeePeek.Moderation.Application.Features.Shop.UpdateShop;

public static class UpdateModerationCoffeeShopHandler
{
    public static async Task<UpdateEntityResponse<ModerationShopDto>> Handle(
        UpdateModerationCoffeeShopCommand command,
        IModerationShopRepository repository,
        IYandexGeocodingService geocodingService,
        IMapper mapper,
        OutgoingMessages outgoingMessages,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var moderationShopDto = command.ModerationShopDto;
        var shop = await repository.GetByIdWithOutDetails(command.ModerationShopDto.Id, ct);

        if (shop == null || shop.UserId != command.UserId)
        {
            return UpdateEntityResponse<ModerationShopDto>.Error("Shop not found or access denied");
        }

        var domainPriceRange = moderationShopDto.PriceRange != null
            ? (DomainPriceRange?)moderationShopDto.PriceRange
            : null;
        shop.UpdateInfo(moderationShopDto.Name, moderationShopDto.Description, domainPriceRange, moderationShopDto.CityId);

        if (moderationShopDto.Address != null && moderationShopDto.Address != shop.Location?.Address)
        {
            var geo = await geocodingService.GeocodeAsync(moderationShopDto.Address, ct);
            if (geo != null) shop.Location!.SetLocation(geo.Latitude, geo.Longitude, moderationShopDto.Address);
        }

        if (moderationShopDto.ShopContact != null)
        {
            shop.UpdateContacts(
                moderationShopDto.ShopContact.PhoneNumber,
                moderationShopDto.ShopContact.InstagramLink,
                moderationShopDto.ShopContact.Email,
                moderationShopDto.ShopContact.SiteLink);
        }

        if (moderationShopDto.Schedules != null)
            shop.UpdateSchedules(moderationShopDto.Schedules.Select(s =>
                (s.DayOfWeek, s.Intervals.Select(i => (i.OpenTime, i.CloseTime)).ToList())));

        shop.UpdateRelations(
            moderationShopDto.EquipmentIds,
            moderationShopDto.CoffeeBeanIds,
            moderationShopDto.RoasterIds,
            moderationShopDto.BrewMethodIds);

        // Publish PhotoReplacedEvent for removed photos
        if (moderationShopDto.ShopPhotos is not null)
        {
            var oldPhotos = shop.ShopPhotos.ToList();

            var newPhotoStorageKeys = moderationShopDto.ShopPhotos
                .Select(p => p.StorageKey)
                .ToHashSet();
            var removedPhotos = oldPhotos
                .Where(p => !newPhotoStorageKeys.Contains(p.StorageKey))
                .ToList();

            foreach (var removedPhoto in removedPhotos)
            {
                outgoingMessages.Add(new PhotoReplacedEvent(
                    removedPhoto.Id,
                    removedPhoto.StorageKey,
                    Guid.Empty,
                    "Shop",
                    shop.Id,
                    DateTime.UtcNow));
            }
        }

        await unitOfWork.SaveChangesAsync(ct);
        
        var result = mapper.Map<ModerationShopDto>(shop);

        return UpdateEntityResponse<ModerationShopDto>.Success(result, "Shop updated successfully");
    }
}
