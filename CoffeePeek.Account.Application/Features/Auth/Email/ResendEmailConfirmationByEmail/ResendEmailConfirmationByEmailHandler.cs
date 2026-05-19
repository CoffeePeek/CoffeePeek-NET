using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Account.Application.Features.Auth.Email.ResendEmailConfirmationByEmail;

public static class ResendEmailConfirmationByEmailHandler
{
    public static async Task<(Response, UserRegisteredInternalEvent?)> Handle(
        ResendEmailConfirmationByEmailCommand request,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var user = await userRepository.GetByEmail(request.Email, ct);

        // Silent success — do not reveal whether email exists or is already confirmed
        if (user is null || user.Credentials.EmailConfirmed)
            return (Response.Success(new { message = "If your email is registered and unconfirmed, a confirmation email is on its way!" }), null);

        user.Credentials.ResetEmailConfirmedFlow();

        await userRepository.Update(user, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var @event = new UserRegisteredInternalEvent(
            user.Id,
            user.Credentials.Email.Value,
            user.Username.Value,
            user.Credentials.EmailConfirmationToken!);

        return (Response.Success(new { message = "If your email is registered and unconfirmed, a confirmation email is on its way!" }), @event);
    }
}
