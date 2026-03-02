using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Account.Application.Features.Auth.Email.ResendEmailConfirmation;

public static class ResendEmailConfirmationHandler
{
    public static async Task<(Response, UserRegisteredInternalEvent)> Handle(
        ResendEmailConfirmationCommand request, 
        IUserRepository userRepository,
        CancellationToken ct)
    {
        var user = await userRepository.GetById(request.UserId, ct)
                   ?? throw new NotFoundException("User not found");

        if (user.Credentials.EmailConfirmed)
            throw new DomainException("Email already confirmed.");

        user.Credentials.ResetEmailConfirmedFlow();
        
        var @event = new UserRegisteredInternalEvent(
            user.Id,
            user.Credentials.Email.Value, 
            user.Username.Value, 
            user.Credentials.EmailConfirmationToken!);

        return (Response.Success(new { message = "Confirmation email is on its way!" }), @event);
    }
}