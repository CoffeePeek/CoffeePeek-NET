using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Events.Account;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using MassTransit;
using Wolverine.Attributes;

namespace CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdateUsername;

public static class UpdateUsernameHandler
{
    public static async Task<UpdateEntityResponse<string>> Handle(
        UpdateProfileUsernameCommand command,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IEventPublisher publishEndpoint,
        CancellationToken ct)
    {
        var user = await userRepository.GetById(command.UserId, ct)
                   ?? throw new NotFoundException("User not found");

        var userName = Username.Create(command.Username);
        user.UpdateUsername(userName);

        await publishEndpoint.Publish(new UserNameChangedEvent
        {
            UserId = user.Id,
            NewUserName = userName.Value,
            ChangedAt = DateTime.UtcNow
        }, ct);

        await unitOfWork.SaveChangesAsync(ct);

        return UpdateEntityResponse<string>.Success(
            userName.ToString(), 
            "Username updated successful");
    }
}