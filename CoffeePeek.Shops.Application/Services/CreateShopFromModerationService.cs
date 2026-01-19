using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Entities;
using CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Shops.Application.Services;

public class CreateShopFromModerationService(
    IGenericRepository<CoffeeShop> shopRepository,
    IGenericRepository<CoffeeBean> coffeeBeanRepository,
    IGenericRepository<Equipment> equipmentRepository,
    IGenericRepository<Roaster> roasterRepository,
    IGenericRepository<BrewMethod> brewMethodRepository,
    IUnitOfWork unitOfWork,
    IOutboxEventPublisher outboxEventPublisher,
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

        var shop = new CoffeeShop(creatorId, shopDto.Name, shopDto.PriceRange, moderationId);
        shop.UpdateDetails(shopDto.Name, shopDto.Description, shopDto.PriceRange);

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
            var equipments = await GetValidEntitiesAsync(equipmentRepository, ids, nameof(Equipment), cancellationToken);
            shop.SetEquipment(equipments);
        }

        if (shopDto.BrewMethods is { Length: > 0 })
        {
            var ids = shopDto.BrewMethods.Select(x => x.Id).ToList();
            var brewMethods = await GetValidEntitiesAsync(brewMethodRepository, ids, nameof(BrewMethod), cancellationToken);
            shop.SetBrewMethods(brewMethods);
        }

        if (shopDto.Roasters is { Length: > 0 })
        {
            var ids = shopDto.Roasters.Select(x => x.Id).ToList();
            var roasters = await GetValidEntitiesAsync(roasterRepository, ids, nameof(Roaster), cancellationToken);
            shop.SetRoasters(roasters);
        }

        if (shopDto.CoffeeBeans is { Length: > 0 })
        {
            var ids = shopDto.CoffeeBeans.Select(x => x.Id).ToList();
            var coffeeBeans = await GetValidEntitiesAsync(coffeeBeanRepository, ids, nameof(CoffeeBean), cancellationToken);
            shop.SetBeans(coffeeBeans);
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

        await outboxEventPublisher.PublishAsync(new ShopCreatedEvent
        {
            ShopId = shop.Id,
            ModerationId = moderationId,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        logger.LogInformation("Shop {ShopId} successfully created from moderation event {ModerationId}", shop.Id, moderationId);
    }

    private async Task<List<T>> GetValidEntitiesAsync<T>(
        IGenericRepository<T> repository, 
        List<Guid> ids, 
        string entityName, 
        CancellationToken ct) where T : Entity<Guid>
    {
        var existing = await repository.Query()
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(ct);

        var missingIds = ids.Except(existing.Select(x => x.Id)).ToList();
        if (missingIds.Count != 0)
            logger.LogWarning("Missing {EntityName} IDs: {Ids}", entityName, string.Join(", ", missingIds));

        return existing;
    }
}

