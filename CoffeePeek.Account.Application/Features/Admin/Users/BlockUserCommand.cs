using System.Net;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Domain.Interfaces.Persistance;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Account.Application.Features.Admin.Users;

public record BlockUserCommand(Guid TargetUserId, bool Blocked);

public static class BlockUserHandler
{
    public static async Task<Response<bool>> Handle(
        BlockUserCommand command,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var user = await userRepository.GetById(command.TargetUserId, ct);
        if (user is null)
            return Response<bool>.Error(HttpStatusCode.NotFound, "User not found.");

        if (user.IsSoftDelete)
            return Response<bool>.Error(HttpStatusCode.BadRequest, "Deleted users cannot be blocked or unblocked.");

        user.SetBlocked(command.Blocked);

        await userRepository.Update(user, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Response<bool>.Success(true);
    }
}
