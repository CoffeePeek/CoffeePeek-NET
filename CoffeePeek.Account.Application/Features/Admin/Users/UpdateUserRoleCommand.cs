using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using CoffeePeek.Account.Domain.Entities.RoleAggregate;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Domain.Interfaces.Persistance;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Account.Application.Features.Admin.Users;

public record UpdateUserRoleCommand(
    [property: JsonIgnore] Guid TargetUserId,
    [Required, MaxLength(50)] string Role);

public static class UpdateUserRoleHandler
{
    public static async Task<Response<Guid>> Handle(
        UpdateUserRoleCommand command,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var user = await userRepository.GetById(command.TargetUserId, ct);
        if (user is null)
            return Response<Guid>.Error(HttpStatusCode.NotFound, "User not found.");

        var role = await roleRepository.GetRoleAsync(command.Role);
        if (role is null)
            return Response<Guid>.Error(HttpStatusCode.BadRequest, $"Role '{command.Role}' not found.");

        user.ReplaceRoles(role);

        await userRepository.Update(user, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Response<Guid>.Success(user.Id);
    }
}
