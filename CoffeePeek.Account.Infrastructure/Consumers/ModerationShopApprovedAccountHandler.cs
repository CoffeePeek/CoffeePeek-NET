using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Shared.Kernel;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Account.Infrastructure.Consumers;

public class ModerationShopApprovedAccountHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ILogger<ModerationShopApprovedAccountHandler> logger)
{
    public async Task Handle(ModerationShopApprovedEvent message)
    {
        logger.LogInformation("Received ModerationShopApprovedEvent for UserId: {UserId}", message.UserId);

        var user = await userRepository.GetById(message.UserId)
                   ?? throw new InvalidOperationException("User not found");

        user.Statistics.IncrementAddedShops();

        logger.LogInformation("Updated UserStatistics for UserId: {UserId}. New AddedShopsCount: {AddedShopsCount}",
            message.UserId, user.Statistics.AddedShopsCount);

        await unitOfWork.SaveChangesAsync();
    }
}