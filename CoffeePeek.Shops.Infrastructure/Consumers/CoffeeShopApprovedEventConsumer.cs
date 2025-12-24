using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Entities;
using MapsterMapper;
using MassTransit;

namespace CoffeePeek.Shops.Infrastructure.Consumers;

public class CoffeeShopApprovedShopsConsumer(
    IMapper mapper,
    IGenericRepository<Shop> shopRepository,
    IUnitOfWork unitOfWork) : IConsumer<CoffeeShopApprovedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<CoffeeShopApprovedIntegrationEvent> context)
    {
        var @event = context.Message;
    
        var shop = new Shop(@event.Shop.Id, @event.Shop.Name, @event.Shop.CityId, @event.Shop.PriceRange);
        shop.UpdateDetails(@event.Shop.Name, @event.Shop.Description, @event.Shop.PriceRange);

        shop.SetLocation(mapper.Map<Location>(@event.Shop.Location!));
        shop.SetContact(mapper.Map<ShopContact>(@event.Shop.ShopContact!));
    
        var photos = @event.Shop.ImageUrls?.Select(url => new ShopPhoto(Guid.NewGuid(), @event.CreatorId, url));
        if (photos != null) shop.AddPhotos(photos);

        await shopRepository.AddAsync(shop, context.CancellationToken);
        await unitOfWork.SaveChangesAsync(context.CancellationToken);
    }
}
