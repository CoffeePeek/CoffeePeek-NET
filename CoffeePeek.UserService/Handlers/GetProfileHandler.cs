using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Contract.Requests.User;
using CoffeePeek.Contract.Response;
using CoffeePeek.UserService.Models;
using CoffeePeek.UserService.Repositories;
using MediatR;

namespace CoffeePeek.UserService.Handlers;

public class GetProfileHandler(IUserRepository userRepository) 
    : IRequestHandler<GetProfileRequest, Response<UserDto>>
{
    public async Task<Response<UserDto>> Handle(GetProfileRequest request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId);

        if (user == null)
        {
            return Response.ErrorResponse<Response<UserDto>>("User not found.");
        }

        var result = MapToDto(user);
        
        return Response.SuccessResponse<Response<UserDto>>(result);
    }

    public static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            UserName = user.Username,
            Email = user.Email,
            Password = string.Empty, // Password не хранится в UserService
            Token = string.Empty, // Token генерируется в AuthService
            Roles = null, // Roles хранятся в AuthService
            About = user.About ?? string.Empty,
            CreatedAt = DateTime.UtcNow, // TODO: добавить CreatedAt в User модель
            PhotoUrl = user.AvatarUrl ?? string.Empty,
            ReviewCount = 0 // Reviews хранятся в другом сервисе
        };
    }
}

