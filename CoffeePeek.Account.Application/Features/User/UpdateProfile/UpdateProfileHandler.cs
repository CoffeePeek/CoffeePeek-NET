using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Events.Account;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Account.Application.Features.User.UpdateProfile;

public class UpdateProfileHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IOutboxEventPublisher outboxEventPublisher,
    ILogger<UpdateProfileHandler> logger)
    : IRequestHandler<UpdateProfileCommand, Response>
{
    public async Task<Response> Handle(
        UpdateProfileCommand command,
        CancellationToken ct)
    {
        var user = await userRepository.GetById(command.UserId, ct);
        if (user == null)
        {
            return Response.Error("User not found");
        }

        var oldUserName = user.Username.Value;
        var userName = Username.Create(command.Username);
        var phoneNumber = PhoneNumber.Create(command.PhoneNumber);
        
        user.UpdateProfile(userName, phoneNumber, command.About);
        
        // Публикуем событие если UserName изменился
        if (userName.Value != oldUserName)
        {
            await outboxEventPublisher.PublishAsync(new UserNameChangedEvent
            {
                UserId = user.Id,
                NewUserName = userName.Value,
                ChangedAt = DateTime.UtcNow
            }, ct);
        }
        
        await userRepository.Update(user, ct);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Profile updated for user {UserId}", user.Id);

        return Response.Success(null, "Profile updated successfully");
    }
}