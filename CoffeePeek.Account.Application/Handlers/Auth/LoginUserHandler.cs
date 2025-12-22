using CoffeePeek.Account.Application.Commands;
using CoffeePeek.Account.Application.Models;
using CoffeePeek.Account.Application.Services;
using CoffeePeek.Account.Domain.Entities;
using CoffeePeek.Contract.Responses.Login;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using MediatR;
using Microsoft.Extensions.Logging;
using SignInResult = CoffeePeek.Account.Application.Models.SignInResult;

namespace CoffeePeek.Account.Application.Handlers;

public class LoginUserHandler(
    IHybridCache cache,
    IUserManager userManager,
    IJWTTokenService jwtTokenService,
    ISignInManager signInManager,
    IUnitOfWork unitOfWork,
    ILogger<LoginUserHandler> logger)
    : IRequestHandler<LoginUserCommand, Contract.Responses.Response<LoginResponse>>
{
    public async Task<Contract.Responses.Response<LoginResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var credentialsByEmailKey = CacheKey.Auth.CredentialsByEmail(request.Email);
        UserCredential? user;
        try
        {
            user = await cache.GetOrSetAsync(
                credentialsByEmailKey,
                () => userManager.FindByEmailAsync(request.Email),
                distributedTtl: credentialsByEmailKey.DefaultTtl,
                cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error finding user by email {Email}", request.Email);
            return Contract.Responses.Response<LoginResponse>.Error(e.Message);
        }
        logger.LogInformation("Attempting to log in user with email: {Email}", request.Email);

        if (user == null)
        {
            logger.LogWarning("Login failed: Account with email {Email} does not exist.", request.Email);
            return Contract.Responses.Response<LoginResponse>.Error("Account does not exist.");
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
        await cache.SetAsync(CacheKey.Auth.Credentials(user.Id), user, cancellationToken: cancellationToken);
        await cache.SetAsync(credentialsByEmailKey, user, distributedTtl: TimeSpan.FromMinutes(5), cancellationToken: cancellationToken);

        logger.LogInformation("Login successful for user {UserId}.", user.Id);
        var result = new LoginResponse(authResult.AccessToken, authResult.RefreshToken);
        return Contract.Responses.Response<LoginResponse>.Success(result);
    }
}