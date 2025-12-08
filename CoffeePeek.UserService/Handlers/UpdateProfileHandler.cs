using CoffeePeek.Contract.Requests.User;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.User;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.UserService.Repositories;
using MediatR;

namespace CoffeePeek.UserService.Handlers;

public class UpdateProfileHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) 
    : IRequestHandler<UpdateProfileRequest, Response<UpdateProfileResponse>>
{
    public async Task<Response<UpdateProfileResponse>> Handle(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId);

        if (user == null)
        {
            return Response<UpdateProfileResponse>.Error("User not found");
        }

        if (!string.IsNullOrWhiteSpace(request.UserName))
        {
            user.Username = request.UserName.Trim();
        }

        if (request.About is not null)
        {
            user.About = request.About;
        }

        await userRepository.UpdateAsync(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Response<UpdateProfileResponse>.Success(new UpdateProfileResponse(), "Profile updated successfully");
    }
}


