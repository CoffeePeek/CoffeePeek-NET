using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Kernel.Response;
using MapsterMapper;

namespace CoffeePeek.Account.Application.Features.User.GetProfile;

public class GetProfileHandler
{
    public static async Task<Response<UserProfileResponse>> Handle(GetProfileCommand command, IUserRepository userRepository, IMapper mapper, CancellationToken ct)
    {
        var user = await userRepository.GetById(command.UserId, ct);

        if (user == null)
        {
            return Response<UserProfileResponse>.Error("User not found.");
        }
        
        
        var result = mapper.Map<UserProfileResponse>(user);
        
        return Response<UserProfileResponse>.Success(result);
    }
}