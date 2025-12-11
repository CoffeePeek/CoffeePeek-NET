using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.ShopsService.Entities;
using MassTransit;

namespace CoffeePeek.ShopsService.Consumers;

public class CoffeeShopApprovedEventConsumer(
    IGenericRepository<Shop> shopRepository,
    IGenericRepository<ShopContact> shopContactsRepository,
    IGenericRepository<ShopPhoto> shopPhotoRepository,
    IGenericRepository<Location> locationRepository,
    IUnitOfWork unitOfWork) : IConsumer<CoffeeShopApprovedEvent>
{
    /// <summary>
    /// Обрабатывает событие подтверждения кофейни: создаёт запись Shop и при наличии данных создаёт связанные Location, ShopContact и ShopPhoto, затем сохраняет изменения в репозиториях.
    /// </summary>
    /// <param name="consumeContext">Контекст сообщения MassTransit, содержащий экземпляр CoffeeShopApprovedEvent и CancellationToken для отмены операции.</param>
    public async Task Consume(ConsumeContext<CoffeeShopApprovedEvent> consumeContext)
    {
        var @event = consumeContext.Message;
        var cancellationToken = consumeContext.CancellationToken;

        // Создаем Shop
        var shop = new Shop
        {
            Id = Guid.NewGuid(),
            Name = @event.Name,
            CityId = Guid.Empty // TODO: Get CityId from event or other source
        };

        await shopRepository.AddAsync(shop, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Создаем Location с координатами, если они есть
        if (@event.Latitude.HasValue && @event.Longitude.HasValue)
        {
            var location = new Location
            {
                Id = Guid.NewGuid(),
                Address = @event.address ?? @event.NotValidatedAddress,
                Latitude = @event.Latitude,
                Longitude = @event.Longitude,
                ShopId = shop.Id
            };

            await locationRepository.AddAsync(location, cancellationToken);
            shop.LocationId = location.Id;
            shopRepository.Update(shop);
        }

        // Создаем ShopContacts если есть
        if (@event.ShopContact != null)
        {
            var shopContacts = new ShopContact
            {
                Id = Guid.NewGuid(),
                ShopId = shop.Id,
                PhoneNumber = @event.ShopContact.PhoneNumber,
                InstagramLink = @event.ShopContact.InstagramLink
            };
            await shopContactsRepository.AddAsync(shopContacts, cancellationToken);
            shop.ShopContactId = shopContacts.Id;
            shopRepository.Update(shop);
        }

        // Создаем ShopPhotos
        if (@event.ShopPhotos != null && @event.ShopPhotos.Any())
        {
            var photos = @event.ShopPhotos.Select(url => new ShopPhoto
            {
                Id = Guid.NewGuid(),
                ShopId = shop.Id,
                Url = url,
                UserId = @event.UserId
            }).ToList();
            
            await shopPhotoRepository.AddRangeAsync(photos, cancellationToken);
        }

        // Сохраняем все изменения одним вызовом
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
