using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Events.Account;
using CoffeePeek.Shared.Extensions.CAP;
using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Infrastructure.Abstract;
using DotNetCore.CAP;
using MediatR;

namespace CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdateUsername;

public class UpdateUsernameHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, ICapPublisher capPublisher)
    : IRequestHandler<UpdateProfileUsernameCommand, UpdateEntityResponse<string>>
{
    public async Task<UpdateEntityResponse<string>> Handle(UpdateProfileUsernameCommand command,
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