using CoffeePeek.Auth.Infrastructure.Configuration;
using CoffeePeek.Contract.Events.Shops;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Auth.Infrastructure.EventConsumer;

public class ReviewAddedEventConsumer(AccountDbContext dbContext) : IConsumer<ReviewAddedEvent>
{
    public async Task Consume(ConsumeContext<ReviewAddedEvent> context)
    {
        var @event = context.Message;

        var statistics = await dbContext.UserStatistics
            .FirstOrDefaultAsync(s => s.UserId == @event.UserId);

        if (statistics == null)
        {
            statistics = new UserService.Models.UserStatistics
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