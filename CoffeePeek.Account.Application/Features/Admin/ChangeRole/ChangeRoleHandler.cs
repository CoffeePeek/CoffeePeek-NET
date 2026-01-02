using CoffeePeek.Account.Domain.Aggregates.UserAggregate;
using CoffeePeek.Account.Domain.Repositories;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MediatR;

namespace CoffeePeek.Account.Application.Features.Admin.ChangeRole;

public class ChangeRoleHandler(
    IUserCredentialsRepository userCredentialsRepository,
    IGenericRepository<Role> roleRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ChangeRoleCommand, Response>
{
    public async Task<Response> Handle(ChangeRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await userCredentialsRepository.GetById(request.UserIdOfChange, cancellationToken);

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