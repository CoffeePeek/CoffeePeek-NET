using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Account.Application.Features.Auth.Email.ConfirmEmail;

public static class ConfirmEmailHandler
{
    public static async Task<Response> Handle(
        ConfirmEmailCommand request, 
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var user = await userRepository.GetByEmailConfirmToken(request.Token, ct)
                   ?? throw new NotFoundException("User not found.");

        user.ConfirmEmail(request.Token);

        await unitOfWork.SaveChangesAsync(ct);

        return Response.Success(new { message = "Email confirmed successfully!" });
    }
}