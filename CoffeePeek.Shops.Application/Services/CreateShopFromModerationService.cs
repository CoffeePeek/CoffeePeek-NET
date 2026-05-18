using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Shops.Application.Services;

public class CreateShopFromModerationService(
    IQueryCoffeeShopRepository shopRepository,
    IQueryCoffeeBeanRepository coffeeBeanRepository,
    IQueryEquipmentRepository equipmentRepository,
    IQueryRoasterRepository roasterRepository,
    IQueryBrewMethodRepository brewMethodRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateShopFromModerationService> logger) : ICreateShopFromModerationService
{
    public async Task<Guid> CreateShopFromApprovedEventAsync(ShopDto shopDto, Guid creatorId, Guid moderationId, CancellationToken cancellationToken = default)
    {
        var exists = await shopRepository.ExistsByModerationId(moderationId, cancellationToken);
        if (exists)
        {
            logger.LogInformation("Shop with ModerationId {ModerationId} already exists, skipping creation", moderationId);
            throw new InvalidOperationException($"Shop with ModerationId {moderationId} already exists");
        }

        var shop = new CoffeeShop(creatorId, shopDto.Name, shopDto.Description, (PriceRange)shopDto.PriceRange, moderationId);

        if (shopDto.Location != null)
        {
            shop.SetLocation(shopDto.CityId, shopDto.Location.Address, shopDto.Location.Latitude!.Value, shopDto.Location.Longitude!.Value);
        }

        if (shopDto.ShopContact != null)
        {
            shop.SetContact(shopDto.ShopContact.InstagramLink, shopDto.ShopContact.Email, shopDto.ShopContact.SiteLink, shopDto.ShopContact.PhoneNumber);
        }

        if (shopDto.Equipments is { Length: > 0 })
        {
            var ids = shopDto.Equipments.Select(x => x.Id).ToList();
            var equipments = await equipmentRepository.GetByIds(ids, cancellationToken);
            foreach (var equipment in equipments)
            {
                shop.AddEquipment(equipment);
            }
        }

        if (shopDto.BrewMethods is { Length: > 0 })
        {
            var ids = shopDto.BrewMethods.Select(x => x.Id).ToList();
            var brewMethods = await brewMethodRepository.GetByIds(ids, cancellationToken);
            shop.SetBrewMethods(brewMethods);
        }

        if (shopDto.Roasters is { Length: > 0 })
        {
            var ids = shopDto.Roasters.Select(x => x.Id).ToList();
            var roasters = await roasterRepository.GetByIds(ids, cancellationToken);
            shop.SetRoasters(roasters);
        }

        if (shopDto.CoffeeBeans is { Length: > 0 })
        {
            var ids = shopDto.CoffeeBeans.Select(x => x.Id).ToList();
            var coffeeBeans = await coffeeBeanRepository.GetByIds(ids, cancellationToken);
            shop.SetBeans(coffeeBeans);
        }

        if (shopDto.Schedules is { Length: > 0 })
        {
            var schedules = shopDto.Schedules
                .Select(x => 
                    ShopSchedule.Create(x.DayOfWeek, x.IsClosed, x.Intervals
                        .Select(i => ShopScheduleInterval.Create(i.OpenTime, i.CloseTime)).ToList()))
                .ToList();
            shop.AddSchedule(schedules);
        }

        if (shopDto.Photos is { Length: > 0 })
        {
            var photos = shopDto.Photos.Select(p => new ShopPhoto(
                p.FileName,
                p.ContentType,
                p.StorageKey,
                p.SizeBytes,
                p.OwnerId)).ToArray();

            shop.AddPhotos(photos);
        }

        shopRepository.Add(shop);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Shop {ShopId} successfully created from moderation event {ModerationId}", shop.Id,
            moderationId);

        return shop.Id;
    }
}

