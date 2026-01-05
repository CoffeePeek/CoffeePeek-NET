using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Shops.Application.Services;
using MassTransit;

namespace CoffeePeek.Shops.Infrastructure.Consumers;

public class CoffeeShopApprovedShopsConsumer(
    ICreateShopFromModerationService createShopService) 
    : IConsumer<CoffeeShopApprovedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<CoffeeShopApprovedIntegrationEvent> context)
    {
        var (creatorId, shopDto) = context.Message;
        
        await createShopService.CreateShopFromApprovedEventAsync(
            shopDto, 
            creatorId, 
            shopDto.Id, 
            context.CancellationToken);
    }
}