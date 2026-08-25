using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Domain.Interfaces.Persistance;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Response;
using Wolverine.Attributes;

namespace CoffeePeek.Account.Application.Features.User.DeleteUser;

public class DeleteUserHandler
{
    public static async Task<Response<bool>> Handle(DeleteUserCommand command, 
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ISessionTerminationNotifier sessionTerminationNotifier,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetById(command.UserId, cancellationToken);

        if (user == null)
        {
            return Response<bool>.Error("User not found");
        }

        user.RevokeAllSessions();
        user.SetSoftDelete();

        await userRepository.Update(user, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await sessionTerminationNotifier.NotifyForceLogoutAsync(user.Id, SessionTerminationReasons.UserDeleted, cancellationToken);
        
        return Response<bool>.Success(true);
    }
}