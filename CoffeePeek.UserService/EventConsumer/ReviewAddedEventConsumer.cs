using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.UserService.Configuration;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.UserService.EventConsumer;

public class ReviewAddedEventConsumer(UserDbContext dbContext) : IConsumer<ReviewAddedEvent>
{
    public async Task Consume(ConsumeContext<ReviewAddedEvent> context)
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
                ReviewCount = 1,
                AddedShopsCount = 0,
                UpdatedAt = DateTime.UtcNow
            };
            dbContext.UserStatistics.Add(statistics);
        }
        else
        {
            statistics.ReviewCount++;
            statistics.UpdatedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync();
    }
}


