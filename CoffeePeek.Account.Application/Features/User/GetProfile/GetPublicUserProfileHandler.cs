using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using MapsterMapper;

namespace CoffeePeek.Account.Application.Features.User.GetProfile;

public class GetPublicUserProfileHandler
{
    public async Task<Response<UserProfileResponse>> Handle(
        GetPublicUserProfileQuery query, 
        IUserRepository userRepository, 
        IMapper mapper, 
        CancellationToken ct)
    {
        var user = await userRepository.GetById(query.UserId, ct);

        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        var result = mapper.Map<UserProfileResponse>(user);
        
        return Response<UserProfileResponse>.Success(result);
    }
}