using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Account.Application.Features.GetProfile;

public class GetProfileHandler(IUserQueries userQueries) 
    : IRequestHandler<GetProfileCommand, Response<UserDto>>
{
    public async Task<Response<UserDto>> Handle(GetProfileCommand command, CancellationToken ct)
    {
        var userDto = await userQueries.GetProfileByIdAsync(command.UserId, ct);
        
        return userDto != null 
            ? Response<UserDto>.Success(userDto) 
            : Response<UserDto>.Error("User not found.");
    }
}