using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Domain.Interfaces.Persistance;
using CoffeePeek.Shared.Kernel.Response;
using Wolverine.Attributes;

namespace CoffeePeek.Account.Application.Features.User.DeleteUser;

public class DeleteUserHandler
{
    [Transactional]
    public static async Task<Response<bool>> Handle(DeleteUserCommand command, 
        IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetById(command.UserId, cancellationToken);

        if (user == null)
        {
            return Response<bool>.Error("User not found");
        }

        user.SetSoftDelete();

        await userRepository.Update(user, cancellationToken);

        return Response<bool>.Success(true);
    }
}