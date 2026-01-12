using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Shops.Application.Services;
using MassTransit;

namespace CoffeePeek.Shops.Infrastructure.Consumers;

public class ModerationShopApprovedConsumer(
    ICreateShopFromModerationService createShopService) 
    : IConsumer<ModerationShopApprovedEvent>
{
    public async Task Consume(ConsumeContext<ModerationShopApprovedEvent> context)
    {
        var shopDto = context.Message.Shop;
        
        await createShopService.CreateShopFromApprovedEventAsync(
            shopDto, 
            context.Message.UserId, 
            shopDto.Id, 
            context.CancellationToken);
    }
}