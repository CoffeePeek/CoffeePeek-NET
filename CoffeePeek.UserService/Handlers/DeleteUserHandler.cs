using CoffeePeek.Contract.Requests.User;
using CoffeePeek.Contract.Response;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.UserService.Repositories;
using MediatR;

namespace CoffeePeek.UserService.Handlers;

public class DeleteUserHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) 
    : IRequestHandler<DeleteUserRequest, Response<bool>>
{
    public async Task<Response<bool>> Handle(DeleteUserRequest request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId);

        if (user == null)
        {
            return Response.ErrorResponse<Response<bool>>("User not found");
        }
        
        user.IsSoftDelete = true;
        
        await userRepository.UpdateAsync(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Response.SuccessResponse<Response<bool>>(true);
    }
}


