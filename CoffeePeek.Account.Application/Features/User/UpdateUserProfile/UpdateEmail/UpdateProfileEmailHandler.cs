using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Domain.Interfaces.Persistance;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using Wolverine.Attributes;

namespace CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdateEmail;

//TODO CP-157 реализовать смену почты с интеграцией с Resend
public class UpdateEmailRequestHandler
{
    [Transactional]
    public static async Task<UpdateEntityResponse<string>> Handle(
        UpdateProfileEmailCommand request, 
        IUserRepository userRepository,
        CancellationToken ct)
    {
        var user = await userRepository.GetById(request.UserId, ct);

        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        user.Credentials.UpdateEmail(request.Email);

        await userRepository.Update(user, ct);

        return UpdateEntityResponse<string>.Success(user.Credentials.Email, "Email updated successfully");
    }
}