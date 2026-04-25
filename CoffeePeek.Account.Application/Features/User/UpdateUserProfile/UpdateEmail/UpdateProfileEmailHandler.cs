using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdateEmail;

//TODO CP-157 реализовать смену почты с интеграцией с Resend
public class UpdateEmailRequestHandler
{
    public static async Task<UpdateEntityResponse<string>> Handle(
        UpdateProfileEmailCommand request, 
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var user = await userRepository.GetById(request.UserId, ct);

        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        user.Credentials.UpdateEmail(request.Email);

        await userRepository.Update(user, ct);
        
        await unitOfWork.SaveChangesAsync(ct);

        return UpdateEntityResponse<string>.Success(user.Credentials.Email, "Email updated successfully");
    }
}