using CoffeePeek.AuthService.Commands;
using CoffeePeek.AuthService.Entities;
using CoffeePeek.AuthService.Services;
using CoffeePeek.Contract.Response;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using MediatR;

namespace CoffeePeek.AuthService.Handlers;

public class CheckUserExistsByEmailRequestHandler(IRedisService redisService, IUserManager userManager)
    : IRequestHandler<CheckUserExistsByEmailCommand, Response<bool>>
{
    public async Task<Response<bool>> Handle(CheckUserExistsByEmailCommand request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKey.Auth.CredentialsByEmail(request.Email);
        var userFromRedis = await redisService.GetAsync<UserCredentials>(cacheKey);

        if (userFromRedis != null)
        {
            return Response<bool>.Success(true);
        }

        var user = await userManager.FindByEmailAsync(request.Email);

        if (user != null)
        {
            await redisService.SetAsync(cacheKey, user);
        }

        return user == null
            ? Response<bool>.Error("User not found")
            : Response<bool>.Success(true);
    }
}