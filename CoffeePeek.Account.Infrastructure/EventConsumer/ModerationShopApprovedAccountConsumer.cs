using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Shared.Kernel;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Account.Infrastructure.EventConsumer;

public class ModerationShopApprovedAccountConsumer(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ILogger<ModerationShopApprovedAccountConsumer> logger) : IConsumer<ModerationShopApprovedEvent>
{
    public async Task Consume(ConsumeContext<ModerationShopApprovedEvent> context)
    {
        var @event = context.Message;
        var ct = context.CancellationToken;

        logger.LogInformation("Received ModerationShopApprovedEvent for UserId: {UserId}", @event.UserId);

        var user = await userRepository.GetById(@event.UserId, ct)
                   ?? throw new InvalidOperationException("User not found");

        user.Statistics.IncrementAddedShops();

        logger.LogInformation("Updated UserStatistics for UserId: {UserId}. New AddedShopsCount: {AddedShopsCount}",
            @event.UserId, user.Statistics.AddedShopsCount);

        await unitOfWork.SaveChangesAsync(ct);
    }
}