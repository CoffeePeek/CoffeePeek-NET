using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.User.Domain.Configuration;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.User.Infrastructure.EventConsumer;

public class CoffeeShopApprovedEventConsumer(
    UserDbContext dbContext, 
    ILogger<CoffeeShopApprovedEventConsumer> logger) 
    : IConsumer<CoffeeShopApprovedEvent>
{
    public async Task Consume(ConsumeContext<CoffeeShopApprovedEvent> context)
    {
        var @event = context.Message;
        
        logger.LogInformation("Received CoffeeShopApprovedEvent for CreatorId: {CreatorId}", @event.CreatorId);

        var statistics = await dbContext.UserStatistics
            .FirstOrDefaultAsync(s => s.UserId == @event.CreatorId);

        if (statistics == null)
        {
            statistics = new UserService.Models.UserStatistics
            {
                UserId = @event.CreatorId,
                CheckInCount = 0,
                ReviewCount = 0,
                AddedShopsCount = 1,
                UpdatedAt = DateTime.UtcNow
            };
            dbContext.UserStatistics.Add(statistics);
            logger.LogInformation("Created new UserStatistics for UserId: {UserId}", @event.CreatorId);
        }
        else
        {
            statistics.AddedShopsCount++;
            statistics.UpdatedAt = DateTime.UtcNow;
            logger.LogInformation("Updated UserStatistics for UserId: {UserId}. New AddedShopsCount: {AddedShopsCount}", @event.CreatorId, statistics.AddedShopsCount);
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("UserStatistics saved successfully for UserId: {UserId}", @event.CreatorId);
    }
}