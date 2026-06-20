using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Domain.Interfaces.Persistance;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Account.Application.Features.Admin.Users;

public record AdminDeleteUserCommand(Guid TargetUserId);

public static class AdminDeleteUserHandler
{
    public static async Task<Response<bool>> Handle(
        AdminDeleteUserCommand command,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var user = await userRepository.GetById(command.TargetUserId, ct);
        if (user is null)
            return Response<bool>.Error("User not found");

        user.SetSoftDelete();
        user.RevokeAllSessions();

        await userRepository.Update(user, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Response<bool>.Success(true);
    }
}
