using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.UserService.Configuration;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.UserService.EventConsumer;

public class CheckinCreatedEventConsumer(UserDbContext dbContext) : IConsumer<CheckinCreatedEvent>
{
    public async Task Consume(ConsumeContext<CheckinCreatedEvent> context)
    {
        var @event = context.Message;

        var statistics = await dbContext.UserStatistics
            .FirstOrDefaultAsync(s => s.UserId == @event.UserId);

        if (statistics == null)
        {
            statistics = new Models.UserStatistics
            {
                UserId = @event.UserId,
                CheckInCount = 1,
                ReviewCount = 0,
                AddedShopsCount = 0,
                UpdatedAt = DateTime.UtcNow
            };
            dbContext.UserStatistics.Add(statistics);
        }
        else
        {
            statistics.CheckInCount++;
            statistics.UpdatedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync();
    }
}
