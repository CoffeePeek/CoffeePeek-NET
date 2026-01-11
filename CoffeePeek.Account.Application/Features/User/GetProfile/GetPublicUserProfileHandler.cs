using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.User;
using MapsterMapper;
using MediatR;

namespace CoffeePeek.Account.Application.Features.User.GetProfile;

public class GetPublicUserProfileHandler(IUserQueries userQueries, IMapper mapper)
    : IRequestHandler<GetPublicUserProfileQuery, Response<UserProfileResponse>>
{
    public async Task<Response<UserProfileResponse>> Handle(GetPublicUserProfileQuery request, CancellationToken ct)
    {
        var userDto = await userQueries.GetProfileByIdAsync(request.UserId, ct);

        if (userDto == null)
        {
            return Response<UserProfileResponse>.Error("User not found.");
        }

        var result = mapper.Map<UserProfileResponse>(userDto);

        return Response<UserProfileResponse>.Success(result);
    }
}