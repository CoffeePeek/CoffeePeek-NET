using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Domain.Interfaces.Persistance;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using Wolverine.Attributes;

namespace CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdateAbout;

public class UpdateProfileHandler
{
    [Transactional]
    public async Task<UpdateEntityResponse<string>> Handle(
        UpdateProfileAboutCommand command,
        IUserRepository userRepository, 
        CancellationToken ct)
    {
        var user = await userRepository.GetById(command.UserId, ct);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        user.UpdateAbout(command.About);

        await userRepository.Update(user, ct);

        return UpdateEntityResponse<string>.Success(user.About, "Profile updated successfully");
    }
}