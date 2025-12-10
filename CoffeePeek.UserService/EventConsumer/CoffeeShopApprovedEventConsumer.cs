using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.UserService.Configuration;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.UserService.EventConsumer;

public class CoffeeShopApprovedEventConsumer(UserDbContext dbContext) : IConsumer<CoffeeShopApprovedEvent>
{
    public async Task Consume(ConsumeContext<CoffeeShopApprovedEvent> context)
    {
        var @event = context.Message;
        
        var statistics = await dbContext.UserStatistics
            .FirstOrDefaultAsync(s => s.UserId == @event.UserId);

        if (statistics == null)
        {
            statistics = new Models.UserStatistics
            {
                UserId = @event.UserId,
                CheckInCount = 0,
                ReviewCount = 0,
                AddedShopsCount = 1,
                UpdatedAt = DateTime.UtcNow
            };
            dbContext.UserStatistics.Add(statistics);
        }
        else
        {
            statistics.AddedShopsCount++;
            statistics.UpdatedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync();
    }
}