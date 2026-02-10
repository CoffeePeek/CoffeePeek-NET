using CoffeePeek.Account.Domain.Entities.RoleAggregate;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using Wolverine.Attributes;

namespace CoffeePeek.Account.Application.Features.Admin.ChangeRole;

public class ChangeRoleHandler
{
    [Transactional]
    public async Task<Response> Handle(
        ChangeRoleCommand request,
        IRoleRepository roleRepository,
        IUserRepository userRepository,
        CancellationToken ct)
    {
        var user = await userRepository.GetById(request.UserIdOfChange, ct)
                   ?? throw new NotFoundException("User not found");

        var role = await roleRepository.GetByIdAsync(request.RoleId, ct)
                   ?? throw new NotFoundException("Role not found");

        user.AssignRole(role);

        return Response.Success();
    }
}