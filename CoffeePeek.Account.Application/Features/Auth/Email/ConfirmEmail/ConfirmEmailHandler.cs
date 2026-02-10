using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Domain.Interfaces.Persistance;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Account.Application.Features.Auth.Email.ConfirmEmail;

public class ConfirmEmailHandler
{
    public static async Task<Response> Handle(ConfirmEmailCommand request, IUserRepository userRepository, IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailConfirmToken(request.Token, cancellationToken);

        if (user == null)
            throw new NotFoundException("User not found.");

        user.ConfirmEmail(request.Token);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Response.Success(new { message = "Email confirmed successfully!" });
    }
}