using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;
using MassTransit;
using Response = CoffeePeek.Shared.Kernel.Response.Response;

namespace CoffeePeek.Account.Application.Features.Auth.Email.ResendEmailConfirmation;

public static class ResendEmailConfirmationHandler
{
    public static async Task<Response> Handle(
        ResendEmailConfirmationCommand request, 
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IEventPublisher publishEndpoint,
        CancellationToken ct)
    {
        var user = await userRepository.GetById(request.UserId, ct)
                   ?? throw new NotFoundException("User not found");

        if (user.Credentials.EmailConfirmed)
            throw new DomainException("Email already confirmed.");

        user.Credentials.ResetEmailConfirmedFlow();
        
        await publishEndpoint.Publish(new UserRegisteredInternalEvent(
            user.Id,
            user.Credentials.Email.Value, 
            user.Username.Value, 
            user.Credentials.EmailConfirmationToken!), ct);

        await unitOfWork.SaveChangesAsync(ct);

        return Response.Success(new { message = "Confirmation email is on its way!" });
    }
}