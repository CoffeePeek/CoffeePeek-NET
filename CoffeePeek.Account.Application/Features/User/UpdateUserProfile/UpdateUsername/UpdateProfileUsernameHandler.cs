using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Events.Account;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using Wolverine.Attributes;

namespace CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdateUsername;

[Transactional]
public class UpdateUsernameHandler
{
    public async Task<(UpdateEntityResponse<string>, UserNameChangedEvent)> Handle(
        UpdateProfileUsernameCommand command,
        IUserRepository userRepository, 
        CancellationToken ct)
    {
        var user = await userRepository.GetById(command.UserId, ct);
        if (user == null) throw new NotFoundException("User not found");

        var userName = Username.Create(command.Username);
        user.UpdateUsername(userName);

        var response = UpdateEntityResponse<string>.Success(
            userName.ToString(), 
            "Username updated successful");

        var @event = new UserNameChangedEvent
        {
            UserId = user.Id,
            NewUserName = userName.Value,
            ChangedAt = DateTime.UtcNow
        };

        return (response, @event);
    }
}