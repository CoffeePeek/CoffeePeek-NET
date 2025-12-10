using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Contract.Requests.User;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using CoffeePeek.UserService.Models;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.UserService.Handlers;

public class GetProfileHandler(
    IGenericRepository<User> userRepository, 
    IRedisService redisService,
    IMapper mapper) 
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

        var userDto = await userRepository
            .QueryAsNoTracking()
            .ProjectToType<UserDto>(mapper.Config)
            .SingleOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);
        
        if (userDto == null)
        {
            return Response<UserDto>.Error("User not found.");
        }

        await redisService.SetAsync(cacheKey, userDto);
        
        return Response<UserDto>.Success(userDto);
    }
}

