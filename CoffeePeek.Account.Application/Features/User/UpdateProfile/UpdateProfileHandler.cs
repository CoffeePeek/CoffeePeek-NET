using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.User;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Account.Application.Features.User.UpdateProfile;

public class UpdateProfileHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateProfileHandler> logger)
    : IRequestHandler<UpdateProfileCommand, Response<UpdateProfileResponse>>
{
    public async Task<Response<UpdateProfileResponse>> Handle(
        UpdateProfileCommand command,
        CancellationToken ct)
    {
        var user = await userRepository.GetById(command.UserId, ct);
        if (user == null)
        {
            return Response<UpdateProfileResponse>.Error("User not found");
        }

        user.UpdateProfile(command.UserName, command.About);
        
        await userRepository.Update(user, ct);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Profile updated for user {UserId}", user.Id);

        return Response<UpdateProfileResponse>.Success(
            new UpdateProfileResponse(),
            "Profile updated successfully");
    }
}