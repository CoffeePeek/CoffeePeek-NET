using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Constants;
using CoffeePeek.Contract.Events;
using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Constants;
using DotNetCore.CAP;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Account.Infrastructure.EventConsumer;

public class ModerationShopApprovedAccountHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ILogger<ModerationShopApprovedAccountHandler> logger) : ICapSubscribe
{
    [CapSubscribe(CapEventNames.Moderation.ShopApproved)]
    public async Task Handle(ModerationShopApprovedEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation("Received ModerationShopApprovedEvent for UserId: {UserId}", @event.UserId);

        var user = await userRepository.GetById(@event.UserId, cancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        user.Statistics.IncrementAddedShops();

        logger.LogInformation("Updated UserStatistics for UserId: {UserId}. New AddedShopsCount: {AddedShopsCount}",
            @event.UserId, user.Statistics.AddedShopsCount);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("UserStatistics saved successfully for UserId: {UserId}", @event.UserId);
    }
}