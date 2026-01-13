using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Auth.Infrastructure.EventConsumer;

public class ModerationShopApprovedAccountConsumer(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ILogger<ModerationShopApprovedAccountConsumer> logger)
    : IConsumer<ModerationShopApprovedEvent>
{
    public async Task Consume(ConsumeContext<ModerationShopApprovedEvent> context)
    {
        logger.LogInformation("Received ModerationShopApprovedEvent for UserId: {UserId}", context.Message.UserId);

        var user = await userRepository.GetById(context.Message.UserId);

        if (user == null)
        {
            logger.LogWarning("User not found");
            return;
        }

        user.Statistics.IncrementAddedShops();

        logger.LogInformation("Updated UserStatistics for UserId: {UserId}. New AddedShopsCount: {AddedShopsCount}",
            context.Message.UserId, user.Statistics.AddedShopsCount);

        await unitOfWork.SaveChangesAsync(context.CancellationToken);
        logger.LogInformation("UserStatistics saved successfully for UserId: {UserId}", context.Message.UserId);
    }
}