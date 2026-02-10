using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Domain.Interfaces.Persistance;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using Wolverine.Attributes;

namespace CoffeePeek.Account.Application.Features.Auth.Email.ConfirmEmail;

public static class ConfirmEmailHandler
{
    [Transactional]
    public static async Task<Response> Handle(
        ConfirmEmailCommand request, 
        IUserRepository userRepository,
        CancellationToken ct)
    {
        var user = await userRepository.GetByEmailConfirmToken(request.Token, ct)
                   ?? throw new NotFoundException("User not found.");

        user.ConfirmEmail(request.Token);

        return Response.Success(new { message = "Email confirmed successfully!" });
    }
}