using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.UserService.Models;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Auth.Infrastructure.EventConsumer;

public class CoffeeShopApprovedEventConsumer(
    IGenericRepository<UserStatistics> userStatisticRepository, 
    IUnitOfWork unitOfWork,
    ILogger<CoffeeShopApprovedEventConsumer> logger) 
    : IConsumer<CoffeeShopApprovedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<CoffeeShopApprovedIntegrationEvent> context)
    {
        var @event = context.Message;
        
        logger.LogInformation("Received CoffeeShopApprovedEvent for CreatorId: {CreatorId}", @event.CreatorId);

        var statistics = await userStatisticRepository
            .FirstOrDefaultAsync(s => s.UserId == @event.CreatorId);

        if (statistics == null)
        {
            statistics = new UserStatistics
            {
                UserId = @event.CreatorId,
                CheckInCount = 0,
                ReviewCount = 0,
                AddedShopsCount = 1,
                UpdatedAt = DateTime.UtcNow
            };
            userStatisticRepository.Add(statistics);
            logger.LogInformation("Created new UserStatistics for UserId: {UserId}", @event.CreatorId);
        }
        else
        {
            statistics.AddedShopsCount++;
            statistics.UpdatedAt = DateTime.UtcNow;
            logger.LogInformation("Updated UserStatistics for UserId: {UserId}. New AddedShopsCount: {AddedShopsCount}", @event.CreatorId, statistics.AddedShopsCount);
        }

        await unitOfWork.SaveChangesAsync();
        logger.LogInformation("UserStatistics saved successfully for UserId: {UserId}", @event.CreatorId);
    }
}