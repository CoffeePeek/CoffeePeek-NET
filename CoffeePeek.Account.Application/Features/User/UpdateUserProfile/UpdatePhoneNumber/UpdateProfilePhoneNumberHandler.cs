using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MediatR;

namespace CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdatePhoneNumber;

public class UpdatePhoneNumberHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateProfilePhoneNumberCommand, UpdateEntityResponse<string>>
{
    public async Task<UpdateEntityResponse<string>> Handle(UpdateProfilePhoneNumberCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetById(request.UserId, ct);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        var phoneNumber = PhoneNumber.Create(request.PhoneNumber);
        user.UpdatePhoneNumber(phoneNumber);

        await userRepository.Update(user, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return UpdateEntityResponse<string>.Success(phoneNumber.ToString(), "Phone number updated successful");
    }
}