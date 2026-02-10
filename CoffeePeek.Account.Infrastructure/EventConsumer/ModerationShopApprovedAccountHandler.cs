using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Events.Moderation;
using Microsoft.Extensions.Logging;
using Wolverine.Attributes;

namespace CoffeePeek.Account.Infrastructure.EventConsumer;

public class ModerationShopApprovedAccountHandler
{
    [Transactional]
    public async Task Handle(
        ModerationShopApprovedEvent @event, 
        IUserRepository userRepository,
        ILogger<ModerationShopApprovedAccountHandler> logger,
        CancellationToken ct)
    {
        logger.LogInformation("Received ModerationShopApprovedEvent for UserId: {UserId}", @event.UserId);

        var user = await userRepository.GetById(@event.UserId, ct)
                   ?? throw new InvalidOperationException("User not found");

        user.Statistics.IncrementAddedShops();

        logger.LogInformation("Updated UserStatistics for UserId: {UserId}. New AddedShopsCount: {AddedShopsCount}",
            @event.UserId, user.Statistics.AddedShopsCount);
    }
}