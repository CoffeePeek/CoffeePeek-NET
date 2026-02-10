using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Events.Account;
using CoffeePeek.Shared.Domain.Interfaces.Persistance;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using Wolverine;
using Wolverine.Attributes;

namespace CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdateUsername;

[Transactional]
public class UpdateUsernameHandler
{
    public async Task<UpdateEntityResponse<string>> Handle(
        UpdateProfileUsernameCommand command,
        IUserRepository userRepository, 
        IUnitOfWork unitOfWork, 
        IMessageBus bus,
        CancellationToken ct)
    {
        using var transaction = unitOfWork.BeginTransactionAsync(ct);

        var user = await userRepository.GetById(command.UserId, ct);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        var userName = Username.Create(command.Username);

        user.UpdateUsername(userName);
        
        await capPublisher.PublishAsync(new UserNameChangedEvent
        {
            UserId = user.Id,
            NewUserName = userName.Value,
            ChangedAt = DateTime.UtcNow
        }, ct);
        
        await userRepository.Update(user, ct);
        await unitOfWork.SaveChangesAsync(ct);
        
        await unitOfWork.CommitTransactionAsync(ct);

        return UpdateEntityResponse<string>.Success(userName.ToString(), "Username updated successful");
    }
}