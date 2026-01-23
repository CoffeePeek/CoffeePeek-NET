using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MediatR;

namespace CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdateAbout;

public class UpdateProfileHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateProfileAboutCommand, UpdateEntityResponse<string>>
{
    public async Task<UpdateEntityResponse<string>> Handle(
        UpdateProfileAboutCommand command,
        CancellationToken ct)
    {
        var user = await userRepository.GetById(command.UserId, ct);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        user.UpdateAbout(command.About);

        await userRepository.Update(user, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return UpdateEntityResponse<string>.Success(user.About, "Profile updated successfully");
    }
}