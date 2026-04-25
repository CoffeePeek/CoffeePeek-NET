using CoffeePeek.Account.Application.Common;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Account.Application.Features.Auth.CheckUserExistsByEmail;

public class CheckUserExistsByEmailRequestHandler
{
    public static async Task<Response<bool>> Handle(CheckUserExistsByEmailCommand command, IQueryUserRepository userRepository,
        EmailExistenceFilter emailExistenceFilter, CancellationToken cancellationToken)
    {
        if (emailExistenceFilter.MightExist(command.Email))
        {
            return Response<bool>.Success(true, "Email exists");
        }

        var userExists = await userRepository.UserExistsByEmail(command.Email, cancellationToken);

        return userExists
            ? Response<bool>.Success(true, "Email exists")
            : throw new NotFoundException("Email does not exist");
    }
}