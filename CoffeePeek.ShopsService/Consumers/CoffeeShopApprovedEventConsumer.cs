using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.ShopsService.Entities;
using MassTransit;

namespace CoffeePeek.ShopsService.Consumers;

public class CoffeeShopApprovedEventConsumer(
    IGenericRepository<Shop> shopRepository,
    IGenericRepository<ShopContact> shopContactsRepository,
    IGenericRepository<ShopPhoto> shopPhotoRepository,
    IUnitOfWork unitOfWork) : IConsumer<CoffeeShopApprovedEvent>
{
    public  Task Consume(ConsumeContext<CoffeeShopApprovedEvent> consumeContext)
    {
        return Task.CompletedTask;
        // var @event = consumeContext.Message;
        // var cancellationToken = consumeContext.CancellationToken;
        //
        // // Создаем Shop
        // var shop = new Shop
        // {
        //     Id = Guid.NewGuid(),
        //     Name = @event.Name,
        //     Address = @event.NotValidatedAddress,
        //     Latitude = @event.Latitude,
        //     Longitude = @event.Longitude
        // };
        //
        // await shopRepository.AddAsync(shop, cancellationToken);
        // await unitOfWork.SaveChangesAsync(cancellationToken);
        //
        // // Получаем числовой идентификатор для связи с сущностями, использующими int ShopId
        // // Используем хеш от Guid для получения int значения
        // var shopIdInt = Math.Abs(shop.Id.GetHashCode());
        //
        // // Создаем ShopContacts если есть
        // if (@event.ShopContact != null)
        // {
        //     var shopContacts = new ShopContact
        //     {
        //         ShopId = shopIdInt,
        //         PhoneNumber = @event.ShopContact.PhoneNumber,
        //         InstagramLink = @event.ShopContact.InstagramLink
        //     };
        //     await shopContactsRepository.AddAsync(shopContacts, cancellationToken);
        // }
        //
        // // Создаем ShopPhotos
        // if (@event.ShopPhotos != null && @event.ShopPhotos.Any())
        // {
        //     var photos = @event.ShopPhotos.Select(url => new ShopPhoto
        //     {
        //         ShopId = shopIdInt,
        //         Url = url,
        //         UserId = @event.UserId
        //     }).ToList();
        //     
        //     await shopPhotoRepository.AddRangeAsync(photos, cancellationToken);
        // }
        //
        // // Создаем Schedules
        // if (@event.Schedules != null && @event.Schedules.Any())
        // {
        //     var schedules = @event.Schedules.Select(s => new Schedule
        //     {
        //         ShopId = shopIdInt,
        //         DayOfWeek = s.DayOfWeek.Value,
        //         OpeningTime = s.OpeningTime,
        //         ClosingTime = s.ClosingTime
        //     }).ToList();
        //     
        //     await scheduleRepository.AddRangeAsync(schedules, cancellationToken);
        // }
        //
        // // Сохраняем все изменения одним вызовом
        // await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

