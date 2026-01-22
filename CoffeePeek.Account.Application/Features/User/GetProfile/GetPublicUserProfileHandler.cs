using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Extensions.Exceptions;
using MapsterMapper;
using MediatR;

namespace CoffeePeek.Account.Application.Features.User.GetProfile;

public class GetPublicUserProfileHandler(IUserRepository userRepository, IMapper mapper)
    : IRequestHandler<GetPublicUserProfileQuery, Response<UserProfileResponse>>
{
    public async Task<Response<UserProfileResponse>> Handle(GetPublicUserProfileQuery request, CancellationToken ct)
    {
        var user = await userRepository.GetById(request.UserId, ct);

        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        var result = mapper.Map<UserProfileResponse>(user);
        
        return Response<UserProfileResponse>.Success(result);
    }
}