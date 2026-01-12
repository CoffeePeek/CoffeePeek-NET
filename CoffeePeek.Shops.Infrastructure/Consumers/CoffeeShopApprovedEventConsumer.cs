using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Shops.Application.Services;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Shops.Infrastructure.Consumers;

public class ModerationShopApprovedConsumer(
    ICreateShopFromModerationService createShopService,
    ILogger<ModerationShopApprovedConsumer> logger)
    : IConsumer<ModerationShopApprovedEvent>
{
    public async Task Consume(ConsumeContext<ModerationShopApprovedEvent> context)
    {
        var shopDto = context.Message.Shop;

        logger.LogInformation("Received ModerationShopApprovedEvent for UserId: {UserId}, ShopId: {ShopId}",
            context.Message.UserId, shopDto.Id);

        await createShopService.CreateShopFromApprovedEventAsync(
            shopDto,
            context.Message.UserId,
            shopDto.Id,
            context.CancellationToken);

        logger.LogInformation("Shop created successfully from moderation event. ShopId: {ShopId}", shopDto.Id);
    }
}