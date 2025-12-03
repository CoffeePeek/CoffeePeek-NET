using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Domain.Databases;
using CoffeePeek.Domain.Entities.Address;
using CoffeePeek.Domain.Entities.Schedules;
using CoffeePeek.Domain.Entities.Shop;
using MassTransit;

namespace CoffeePeek.Shops.Consumers;

public class CoffeeShopApprovedEventConsumer(CoffeePeekDbContext context) : IConsumer<CoffeeShopApprovedEvent>
{
    public async Task Consume(ConsumeContext<CoffeeShopApprovedEvent> consumeContext)
    {
        var @event = consumeContext.Message;

        // Создаем Address если он есть
        Address? address = null;
        if (@event.Address != null)
        {
            address = new Address
            {
                CityId = @event.Address.CityId,
                StreetId = @event.Address.StreetId,
                BuildingNumber = @event.Address.BuildingNumber,
                PostalCode = @event.Address.PostalCode,
                Latitude = @event.Address.Latitude,
                Longitude = @event.Address.Longitude
            };
            context.Addresses.Add(address);
            await context.SaveChangesAsync();
        }
        else if (@event.AddressId.HasValue)
        {
            // Если AddressId указан в событии, используем существующий адрес
            address = await context.Addresses.FindAsync(@event.AddressId.Value);
        }

        if (address == null)
        {
            throw new InvalidOperationException("Address is required to create a Shop");
        }

        // Создаем Shop
        var shop = new Shop
        {
            Name = @event.Name,
            AddressId = address.Id,
            Status = @event.Status
        };

        context.Shops.Add(shop);
        await context.SaveChangesAsync();

        // Создаем ShopContacts если есть
        if (@event.ShopContact != null)
        {
            var shopContacts = new ShopContacts
            {
                ShopId = shop.Id,
                PhoneNumber = @event.ShopContact.PhoneNumber,
                InstagramLink = @event.ShopContact.InstagramLink
            };
            context.ShopContacts.Add(shopContacts);
            shop.ShopContactId = shopContacts.Id;
            await context.SaveChangesAsync();
        }

        // Создаем ShopPhotos
        if (@event.ShopPhotos != null && @event.ShopPhotos.Any())
        {
            var photos = @event.ShopPhotos.Select(url => new ShopPhoto
            {
                ShopId = shop.Id,
                Url = url,
                UserId = @event.UserId
            }).ToList();
            
            context.Set<ShopPhoto>().AddRange(photos);
            await context.SaveChangesAsync();
        }

        // Создаем Schedules
        if (@event.Schedules != null && @event.Schedules.Any())
        {
            var schedules = @event.Schedules.Select(s => new Schedule
            {
                ShopId = shop.Id,
                DayOfWeek = s.DayOfWeek.Value,
                OpeningTime = s.OpeningTime,
                ClosingTime = s.ClosingTime
            }).ToList();
            
            context.Schedules.AddRange(schedules);
            await context.SaveChangesAsync();
        }
    }
}

