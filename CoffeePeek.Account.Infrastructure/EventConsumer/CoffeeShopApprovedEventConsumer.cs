using CoffeePeek.Account.Domain.Aggregates.UserAggregate;
using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Auth.Infrastructure.EventConsumer;

public class ModerationShopApprovedAccountConsumer(
    IGenericRepository<UserStatistics> userStatisticRepository, 
    IUnitOfWork unitOfWork,
    ILogger<ModerationShopApprovedAccountConsumer> logger) 
    : IConsumer<ModerationShopApprovedEvent>
{
    public async Task Consume(ConsumeContext<ModerationShopApprovedEvent> context)
    {
        logger.LogInformation("Received ModerationShopApprovedEvent for UserId: {UserId}", context.Message.UserId);

        var statistics = await userStatisticRepository
            .FirstOrDefaultAsync(s => s.UserId == context.Message.UserId);

        if (statistics == null)
        {
            statistics = new UserStatistics
            {
                UserId = context.Message.UserId,
                CheckInCount = 0,
                ReviewCount = 0,
                AddedShopsCount = 1,
                UpdatedAt = DateTime.UtcNow
            };
            userStatisticRepository.Add(statistics);
            logger.LogInformation("Created new UserStatistics for UserId: {UserId}", context.Message);
        }
        else
        {
            statistics.AddedShopsCount++;
            statistics.UpdatedAt = DateTime.UtcNow;
            logger.LogInformation("Updated UserStatistics for UserId: {UserId}. New AddedShopsCount: {AddedShopsCount}", context.Message.UserId, statistics.AddedShopsCount);
        }

        await unitOfWork.SaveChangesAsync(context.CancellationToken);
        logger.LogInformation("UserStatistics saved successfully for UserId: {UserId}", context.Message.UserId);
    }
}