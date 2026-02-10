using CoffeePeek.Account.Domain.Entities.RoleAggregate;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Domain.Interfaces.Persistance;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Account.Application.Features.Admin.ChangeRole;

public class ChangeRoleHandler
{
    public static async Task<Response> Handle(
        ChangeRoleCommand request,
        IGenericRepository<Role> roleRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetById(request.UserIdOfChange, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        var role = await roleRepository.GetByIdAsync(request.RoleId, cancellationToken);

        if (role == null)
        {
            throw new NotFoundException("Role not found");
        }

        user.AssignRole(role);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Response.Success();
    }
}