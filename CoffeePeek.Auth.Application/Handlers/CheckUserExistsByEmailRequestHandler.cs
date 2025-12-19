using CoffeePeek.Auth.Application.Commands;
using CoffeePeek.AuthService.Services;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Auth.Application.Handlers;

public class CheckUserExistsByEmailRequestHandler(
    IHybridCache cache,
    IUserManager userManager,
    ILogger<CheckUserExistsByEmailRequestHandler> logger)
    : IRequestHandler<CheckUserExistsByEmailCommand, Response<bool>>
{
    public async Task<Response<bool>> Handle(CheckUserExistsByEmailCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Checking if user with email {Email} exists.", request.Email);
        var cacheKey = CacheKey.Auth.CredentialsByEmail(request.Email);
        var user = await cache.GetOrSetAsync(
            cacheKey,
            () => userManager.FindByEmailAsync(request.Email),
            distributedTtl: cacheKey.DefaultTtl,
            cancellationToken: cancellationToken);

        logger.LogInformation("User with email {Email} {Status} found.", request.Email, user == null ? "not" : "");
        return user == null
            ? Response<bool>.Error("User not found")
            : Response<bool>.Success(true);
    }
}