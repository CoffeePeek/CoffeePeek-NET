using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Application.Services;
using CoffeePeek.Shops.Domain.Entities;
using MapsterMapper;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Shops.Infrastructure.Consumers;

public class CoffeeShopApprovedEventConsumer(
    IMapper mapper,
    IGenericRepository<Shop> shopRepository,
    IGenericRepository<ShopContact> shopContactsRepository,
    IGenericRepository<Location> locationRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    ILogger<CoffeeShopApprovedEventConsumer> logger) : IConsumer<CoffeeShopApprovedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<CoffeeShopApprovedIntegrationEvent> consumeContext)
    {
        var @event = consumeContext.Message;
        var cancellationToken = consumeContext.CancellationToken;
        logger.LogInformation("Received CoffeeShopApprovedEvent for shop: {ShopName}", @event.Shop.Name);

        var shop = mapper.Map<Shop>(@event.Shop);

        await shopRepository.AddAsync(shop, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Shop {ShopName} (ID: {ShopId}) added to repository.", shop.Name, shop.Id);

        if (shop.Location != null)
        {
            shop.LocationId = Guid.NewGuid();
            shop.Location.Id = shop.LocationId.Value;
            shop.Location.ShopId = shop.Id;

            logger.LogInformation("Adding location for shop {ShopName} (ID: {ShopId}).", shop.Name, shop.Id);
            
            await locationRepository.AddAsync(shop.Location, cancellationToken);
            shop.LocationId = shop.Location.Id;
            shopRepository.Update(shop);
            logger.LogInformation("Location (ID: {LocationId}) added and linked to shop {ShopId}.", shop.Location.Id, shop.Id);
        }
        else
        {
            logger.LogWarning("No location provided for shop {ShopName} (ID: {ShopId}).", shop.Name, shop.Id);
        }

        if (shop.ShopContact != null)
        {
            shop.ShopContact.Id = Guid.NewGuid();
            shop.ShopContact.ShopId = shop.Id;
            
            logger.LogInformation("Adding shop contact for shop {ShopName} (ID: {ShopId}).", shop.Name, shop.Id);
            await shopContactsRepository.AddAsync(shop.ShopContact, cancellationToken);
            shop.ShopContactId = shop.ShopContact.Id;
            shopRepository.Update(shop);
            logger.LogInformation("Shop contact (ID: {ShopContactId}) added and linked to shop {ShopId}.", shop.ShopContact.Id, shop.Id);
        }
        else
        {
            logger.LogWarning("No shop contact provided for shop {ShopName} (ID: {ShopId}).", shop.Name, shop.Id);
        }

        //TODO add implementation
        // Создаем ShopPhotos
        //if (shop.ShopPhotos != null)
        //{
        //    var photos = @event.ShopPhotos.Select(url => new ShopPhoto
        //    {
        //        Id = Guid.NewGuid(),
        //        ShopId = shop.Id,
        //        Url = url,
        //        UserId = @event.UserId
        //    }).ToList();
        //    
        //    await shopPhotoRepository.AddRangeAsync(photos, cancellationToken);
        //}
        
        // Сохраняем все изменения одним вызовом
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("All changes saved for shop {ShopName} (ID: {ShopId}).", shop.Name, shop.Id);

        // Invalidate cached shop dictionaries and lists to reflect new shop data
        await cacheService.InvalidateShopDictionaries(cancellationToken);
        await cacheService.InvalidateShopLists(cancellationToken);
    }
}
