using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Shops.Application.Services;

public class CreateShopFromModerationService(
    IGenericRepository<Shop> shopRepository,
    IGenericRepository<CoffeeBean> coffeeBeanRepository,
    IGenericRepository<Equipment> equipmentRepository,
    IGenericRepository<Roaster> roasterRepository,
    IGenericRepository<BrewMethod> brewMethodRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateShopFromModerationService> logger) : ICreateShopFromModerationService
{
    public async Task CreateShopFromApprovedEventAsync(ShopDto shopDto, Guid creatorId, Guid moderationId, CancellationToken cancellationToken = default)
    {
        var exists = await shopRepository.AnyAsync(x => x.ModerationId == moderationId, cancellationToken);
        if (exists)
        {
            logger.LogInformation("Shop with ModerationId {ModerationId} already exists, skipping creation", moderationId);
            return;
        }

        var shop = new Shop(creatorId, shopDto.Name, shopDto.CityId, shopDto.PriceRange, moderationId);
        shop.UpdateDetails(shopDto.Name, shopDto.Description, shopDto.PriceRange);

        if (shopDto.Location != null)
        {
            var location = new Location(shop.Id, shopDto.Location.Address, shopDto.Location.Latitude!.Value, shopDto.Location.Longitude!.Value);
            shop.SetLocation(location);
        }

        if (shopDto.ShopContact != null)
        {
            shop.SetContact(shopDto.ShopContact, shop.Id);
        }

        if (shopDto.Equipments is { Length: > 0 })
        {
            var validEquipmentIds = await ValidateEquipmentIdsAsync(
                shopDto.Equipments.Select(e => e.Id).ToList(),
                cancellationToken);
            
            if (validEquipmentIds.Count != 0)
            {
                shop.SetEquipment(validEquipmentIds);
            }
        }

        if (shopDto.BrewMethods is { Length: > 0 })
        {
            var validBrewMethodIds = await ValidateBrewMethodIdsAsync(
                shopDto.BrewMethods.Select(b => b.Id).ToList(),
                cancellationToken);
            
            if (validBrewMethodIds.Any())
            {
                shop.SetBrewMethods(validBrewMethodIds);
            }
        }

        if (shopDto.Roasters is { Length: > 0 })
        {
            var validRoasterIds = await ValidateRoasterIdsAsync(
                shopDto.Roasters.Select(r => r.Id).ToList(),
                cancellationToken);
            
            if (validRoasterIds.Any())
            {
                shop.SetRoasters(validRoasterIds);
            }
        }

        if (shopDto.Beans is { Length: > 0 })
        {
            var validCoffeeBeanIds = await ValidateCoffeeBeanIdsAsync(
                shopDto.Beans.Select(b => b.Id).ToList(),
                cancellationToken);
            
            if (validCoffeeBeanIds.Any())
            {
                shop.SetBeans(validCoffeeBeanIds);
            }
        }

        if (shopDto.Photos is { Length: > 0 })
        {
            var photos = shopDto.Photos.Select(p => new ShopPhoto(
                p.FileName,
                p.ContentType,
                p.StorageKey,
                p.SizeBytes,
                p.OwnerId,
                shop.Id)).ToArray();

            shop.AddPhotos(photos);
        }

        await shopRepository.AddAsync(shop, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Shop {ShopId} successfully created from moderation event {ModerationId}", shop.Id, moderationId);
    }

    private async Task<List<Guid>> ValidateEquipmentIdsAsync(
        List<Guid> ids,
        CancellationToken cancellationToken)
    {
        var existingIds = await equipmentRepository
            .QueryAsNoTracking()
            .Where(e => ids.Contains(e.Id))
            .Select(e => e.Id)
            .ToListAsync(cancellationToken);

        var missingIds = ids.Except(existingIds).ToList();
        if (missingIds.Any())
        {
            logger.LogWarning("Some Equipment IDs do not exist in ShopsService database: {MissingIds}",
                string.Join(", ", missingIds));
        }

        return existingIds;
    }

    private async Task<List<Guid>> ValidateBrewMethodIdsAsync(
        List<Guid> ids,
        CancellationToken cancellationToken)
    {
        var existingIds = await brewMethodRepository
            .QueryAsNoTracking()
            .Where(b => ids.Contains(b.Id))
            .Select(b => b.Id)
            .ToListAsync(cancellationToken);

        var missingIds = ids.Except(existingIds).ToList();
        if (missingIds.Any())
        {
            logger.LogWarning("Some BrewMethod IDs do not exist in ShopsService database: {MissingIds}",
                string.Join(", ", missingIds));
        }

        return existingIds;
    }

    private async Task<List<Guid>> ValidateRoasterIdsAsync(
        List<Guid> ids,
        CancellationToken cancellationToken)
    {
        var existingIds = await roasterRepository
            .QueryAsNoTracking()
            .Where(r => ids.Contains(r.Id))
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        var missingIds = ids.Except(existingIds).ToList();
        if (missingIds.Any())
        {
            logger.LogWarning("Some Roaster IDs do not exist in ShopsService database: {MissingIds}",
                string.Join(", ", missingIds));
        }

        return existingIds;
    }

    private async Task<List<Guid>> ValidateCoffeeBeanIdsAsync(
        List<Guid> ids,
        CancellationToken cancellationToken)
    {
        var existingIds = await coffeeBeanRepository
            .QueryAsNoTracking()
            .Where(b => ids.Contains(b.Id))
            .Select(b => b.Id)
            .ToListAsync(cancellationToken);

        var missingIds = ids.Except(existingIds).ToList();
        if (missingIds.Any())
        {
            logger.LogWarning("Some CoffeeBean IDs do not exist in ShopsService database: {MissingIds}",
                string.Join(", ", missingIds));
        }

        return existingIds;
    }
}

