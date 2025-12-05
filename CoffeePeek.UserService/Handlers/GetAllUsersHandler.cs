using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Contract.Requests.User;
using CoffeePeek.Contract.Response;
using CoffeePeek.UserService.Repositories;
using MediatR;

namespace CoffeePeek.UserService.Handlers;

public class GetAllUsersHandler(IUserRepository userRepository) 
    : IRequestHandler<GetAllUsersRequest, Response<UserDto[]>>
{
    public async Task<Response<UserDto[]>> Handle(GetAllUsersRequest request, CancellationToken cancellationToken)
    {
        var users = await userRepository.GetAllAsync();
        
        var result = users.Select(GetProfileHandler.MapToDto).ToArray();
        
        return Response.SuccessResponse<Response<UserDto[]>>(result);
    }
}

