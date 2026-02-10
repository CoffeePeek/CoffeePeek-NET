using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Domain.Interfaces.Persistance;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using Wolverine.Attributes;

namespace CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdatePhoneNumber;

public class UpdatePhoneNumberHandler
{
    [Transactional]
    public static async Task<UpdateEntityResponse<string>> Handle(
        UpdateProfilePhoneNumberCommand request, 
        IUserRepository userRepository, 
        CancellationToken ct)
    {
        var user = await userRepository.GetById(request.UserId, ct);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        var phoneNumber = PhoneNumber.Create(request.PhoneNumber);
        user.UpdatePhoneNumber(phoneNumber);

        await userRepository.Update(user, ct);

        return UpdateEntityResponse<string>.Success(phoneNumber.ToString(), "Phone number updated successful");
    }
}