using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.User;
using MapsterMapper;
using MediatR;

namespace CoffeePeek.Account.Application.Features.User.GetProfile;

public class GetProfileHandler(IUserQueries userQueries, IMapper mapper) 
    : IRequestHandler<GetProfileCommand, Response<UserProfileResponse>>
{
    public async Task<Response<UserProfileResponse>> Handle(GetProfileCommand command, CancellationToken ct)
    {
        var userDto = await userQueries.GetProfileByIdAsync(command.UserId, ct);

        if (userDto == null)
        {
            return Response<UserProfileResponse>.Error("User not found.");
        }
        var result = mapper.Map<UserProfileResponse>(userDto);
        
        return Response<UserProfileResponse>.Success(result);
    }
}