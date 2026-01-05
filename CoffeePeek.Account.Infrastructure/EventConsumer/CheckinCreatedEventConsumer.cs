using CoffeePeek.Account.Domain.Aggregates.UserAggregate;
using CoffeePeek.Auth.Infrastructure.Persistent;
using CoffeePeek.Contract.Events.Shops;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Auth.Infrastructure.EventConsumer;

public class CheckinCreatedEventConsumer(AccountDbContext dbContext) : IConsumer<CheckinCreatedEvent>
{
    public async Task Consume(ConsumeContext<CheckinCreatedEvent> context)
    {
        var @event = context.Message;

        var statistics = await dbContext.UserStatistics
            .FirstOrDefaultAsync(s => s.UserId == @event.UserId);

        if (statistics == null)
        {
            statistics = new UserStatistics
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