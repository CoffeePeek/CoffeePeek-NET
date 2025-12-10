using CoffeePeek.AuthService.Commands;
using CoffeePeek.AuthService.Entities;
using CoffeePeek.AuthService.Services;
using CoffeePeek.Contract.Events;
using CoffeePeek.Contract.Response.Login;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using MassTransit;
using MediatR;
using SignInResult = CoffeePeek.AuthService.Models.SignInResult;

namespace CoffeePeek.AuthService.Handlers;

public class LoginUserHandler(
    IRedisService redisService,
    IUserManager userManager,
    IJWTTokenService jwtTokenService,
    ISignInManager signInManager,
    IPublishEndpoint publishEndpoint,
    ILogger logger)
    : IRequestHandler<LoginUserCommand, Contract.Responses.Response<LoginResponse>>
{
    public async Task<Contract.Responses.Response<LoginResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var credentialsByEmailKey = CacheKey.Auth.CredentialsByEmail(request.Email);
        var user = await redisService.GetAsync<UserCredentials>(credentialsByEmailKey);

        if (user == null)
        {
            try
            {
                user = await userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return Contract.Responses.Response<LoginResponse>.Error("Account does not exist.");
                }
            }
            catch (Exception e)
            {
                return Contract.Responses.Response<LoginResponse>.Error(e.Message);
            }
        }

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, request.Password);
        if (signInResult.Result != SignInResult.Success)
        {
            return Contract.Responses.Response<LoginResponse>.Error("Password is incorrect.");
        }

        var authResult = await jwtTokenService.GenerateTokensAsync(user);

        await redisService.SetAsync(CacheKey.Auth.Credentials(user.Id), user);
        await redisService.SetAsync(credentialsByEmailKey, user, TimeSpan.FromMinutes(5));
        
        _ = Task.Run(async () =>
        {
            try
            {
                await publishEndpoint.Publish(new UserLoggedInEvent(user.Id), cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to publish UserLoggedInEvent");
            }
        }, cancellationToken);

        var result = new LoginResponse(authResult.AccessToken, authResult.RefreshToken);
        return Contract.Responses.Response<LoginResponse>.Success(result);
    }
}