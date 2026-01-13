using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.User;
using MapsterMapper;
using MediatR;

namespace CoffeePeek.Account.Application.Features.User.GetProfile;

public class GetProfileHandler(IUserRepository userRepository, IMapper mapper) 
    : IRequestHandler<GetProfileCommand, Response<UserProfileResponse>>
{
    public async Task<Response<UserProfileResponse>> Handle(GetProfileCommand command, CancellationToken ct)
    {
        var user = await userRepository.GetById(command.UserId, ct);

        if (user == null)
        {
            return Response<UserProfileResponse>.Error("User not found.");
        }
        
        var userDto = mapper.Map<UserDto>(user);
        
        var result = mapper.Map<UserProfileResponse>(userDto);
        
        return Response<UserProfileResponse>.Success(result);
    }
}