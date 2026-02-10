using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using Wolverine.Attributes;

namespace CoffeePeek.Account.Application.Features.Auth.Email.ResendEmailConfirmation;

public static class ResendEmailConfirmationHandler
{
    [Transactional]
    public static async Task<(Response, UserRegisteredInternalEvent)> Handle(
        ResendEmailConfirmationCommand request, 
        IUserRepository userRepository,
        CancellationToken ct)
    {
        var user = await userRepository.GetById(request.UserId, ct)
                   ?? throw new NotFoundException("User not found");

        if (user.Credentials.EmailConfirmed)
            throw new DomainException("Email already confirmed.");


        var response = Response.Success(new { message = "Confirmation email is on its way!" });

        var @event = new UserRegisteredInternalEvent(
            user.Id,
            user.Credentials.Email.Value, 
            user.Username.Value, 
            user.Credentials.EmailConfirmationToken!);

        return (response, @event);
    }
}