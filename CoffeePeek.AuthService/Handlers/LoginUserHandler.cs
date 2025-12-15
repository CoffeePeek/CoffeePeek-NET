using CoffeePeek.AuthService.Commands;
using CoffeePeek.AuthService.Entities;
using CoffeePeek.AuthService.Services;
using CoffeePeek.Contract.Events;
using CoffeePeek.Contract.Responses.Login;
using CoffeePeek.Data.Interfaces;
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
    IUnitOfWork unitOfWork,
    ILogger<LoginUserHandler> logger)
    : IRequestHandler<LoginUserCommand, Contract.Responses.Response<LoginResponse>>
{
    public async Task<Contract.Responses.Response<LoginResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var credentialsByEmailKey = CacheKey.Auth.CredentialsByEmail(request.Email);
        var user = await redisService.GetAsync<UserCredentials>(credentialsByEmailKey);
        logger.LogInformation("Attempting to log in user with email: {Email}", request.Email);

        if (user == null)
        {
            try
            {
                user = await userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    logger.LogWarning("Login failed: Account with email {Email} does not exist.", request.Email);
                    return Contract.Responses.Response<LoginResponse>.Error("Account does not exist.");
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error finding user by email {Email}", request.Email);
                return Contract.Responses.Response<LoginResponse>.Error(e.Message);
            }
        }

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, request.Password);
        if (signInResult.Result != SignInResult.Success)
        {
            logger.LogWarning("Login failed for user {UserId}: Incorrect password.", user.Id);
            return Contract.Responses.Response<LoginResponse>.Error("Password is incorrect.");
        }

        var authResult = await jwtTokenService.GenerateTokensAsync(
            user,
            request.DeviceName,
            request.IpAddress);
        
        // Save refresh token to database
        logger.LogInformation("Saving refresh token for user {UserId} to database.", user.Id);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Caching user credentials for user {UserId} and email {Email}.", user.Id, request.Email);
        await redisService.SetAsync(CacheKey.Auth.Credentials(user.Id), user);
        await redisService.SetAsync(credentialsByEmailKey, user, TimeSpan.FromMinutes(5));
        logger.LogInformation("User {UserId} logged in successfully. Publishing UserLoggedInEvent.", user.Id);
        
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

        logger.LogInformation("Login successful for user {UserId}.", user.Id);
        var result = new LoginResponse(authResult.AccessToken, authResult.RefreshToken);
        return Contract.Responses.Response<LoginResponse>.Success(result);
    }
}