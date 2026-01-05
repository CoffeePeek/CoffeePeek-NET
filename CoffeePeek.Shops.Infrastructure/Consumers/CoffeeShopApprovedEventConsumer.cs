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
        var (_, dto) = context.Message;

        var shop = new Shop(dto.Id, dto.Name, dto.CityId, dto.PriceRange);
        shop.UpdateDetails(dto.Name, dto.Description, dto.PriceRange);

        shop.SetLocation(mapper.Map<Location>(dto.Location!));
        shop.SetContact(mapper.Map<ShopContact>(dto.ShopContact!));

        
        if (dto.Photos is { Length: > 0 })
        {
            var photos = dto.Photos.Select(p => new ShopPhoto(
                p.FileName, 
                p.ContentType, 
                p.StorageKey, 
                p.SizeBytes, 
                p.OwnerId, 
                shop.Id)).ToArray();
            
            shop.AddPhotos(photos);
        }

        await shopRepository.AddAsync(shop, context.CancellationToken);
        await unitOfWork.SaveChangesAsync(context.CancellationToken);
    }
}
