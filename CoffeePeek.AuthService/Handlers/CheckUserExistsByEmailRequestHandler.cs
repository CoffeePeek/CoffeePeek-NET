using CoffeePeek.AuthService.Commands;
using CoffeePeek.AuthService.Entities;
using CoffeePeek.AuthService.Services;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using MediatR;

namespace CoffeePeek.AuthService.Handlers;

public class CheckUserExistsByEmailRequestHandler(
    IRedisService redisService,
    IUserManager userManager,
    ILogger<CheckUserExistsByEmailRequestHandler> logger)
    : IRequestHandler<CheckUserExistsByEmailCommand, Response<bool>>
{
    public async Task<Response<bool>> Handle(CheckUserExistsByEmailCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Checking if user with email {Email} exists.", request.Email);
        var cacheKey = CacheKey.Auth.CredentialsByEmail(request.Email);
        var userFromRedis = await redisService.GetAsync<UserCredentials>(cacheKey);

        if (userFromRedis != null)
        {
            logger.LogInformation("User with email {Email} found in cache.", request.Email);
            return Response<bool>.Success(true);
        }

        logger.LogInformation("User with email {Email} not found in cache. Checking database.", request.Email);
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user != null)
        {
            logger.LogInformation("User with email {Email} found in database. Caching user.", request.Email);
            await redisService.SetAsync(cacheKey, user);
        }

        logger.LogInformation("User with email {Email} {Status} found.", request.Email, user == null ? "not" : "");
        return user == null
            ? Response<bool>.Error("User not found")
            : Response<bool>.Success(true);
    }
}