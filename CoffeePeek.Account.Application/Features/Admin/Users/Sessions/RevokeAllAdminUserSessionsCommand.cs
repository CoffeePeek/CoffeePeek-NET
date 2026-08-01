using System.Net;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Account.Application.Features.Admin.Users.Sessions;

public record RevokeAllAdminUserSessionsCommand(Guid UserId);

public static class RevokeAllAdminUserSessionsHandler
{
    public static async Task<Response<bool>> Handle(
        RevokeAllAdminUserSessionsCommand command,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var user = await userRepository.GetById(command.UserId, ct);
        if (user is null)
            return Response<bool>.Error(HttpStatusCode.NotFound, "User not found.");

        user.RevokeAllSessions();

        await userRepository.Update(user, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Response<bool>.Success(true);
    }
}
