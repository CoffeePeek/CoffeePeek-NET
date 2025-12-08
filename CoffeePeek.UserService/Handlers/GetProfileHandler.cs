using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Contract.Requests.User;
using CoffeePeek.Contract.Response;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using CoffeePeek.UserService.Models;
using CoffeePeek.UserService.Repositories;
using MediatR;

namespace CoffeePeek.UserService.Handlers;

public class GetProfileHandler(IUserRepository userRepository, IRedisService redisService) 
    : IRequestHandler<GetProfileRequest, Response<UserDto>>
{
    public async Task<Response<UserDto>> Handle(GetProfileRequest request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKey.User.Profile(request.UserId);
        var cachedUser = await redisService.GetAsync<UserDto>(cacheKey);
        if (cachedUser != null)
        {
            return Response<UserDto>.Success(cachedUser);
        }
        
        var user = await userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            return Response<UserDto>.Error("User not found.");
        }

        var result = MapToDto(user);
        
        await redisService.SetAsync(cacheKey, result);
        
        return Response<UserDto>.Success(result);
    }

    public static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            UserName = user.Username,
            Email = user.Email,
            About = user.About ?? string.Empty,
            PhotoUrl = user.AvatarUrl ?? string.Empty,
        };
    }
}

