using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MediatR;

namespace CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdateEmail;

//TODO CP-157 реализовать смену почты с интеграцией с Resend
public class UpdateEmailRequestHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateProfileEmailCommand, UpdateEntityResponse<string>>
{
    public async Task<UpdateEntityResponse<string>> Handle(UpdateProfileEmailCommand request, CancellationToken ct)
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