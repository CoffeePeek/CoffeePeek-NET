using CoffeePeek.Account.Application.Commands;
using CoffeePeek.Account.Application.Services;
using CoffeePeek.Account.Domain.Entities;
using CoffeePeek.Account.Domain.Repositories;
using CoffeePeek.Contract.Responses.Auth;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Constants;
using CoffeePeek.User.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Account.Application.Handlers;

public class GoogleLoginHandler(
    IGoogleAuthService googleAuthService,
    IUserCredentialsRepository userRepository,
    IUserRepository userProfileRepository,
    IUserManager userManager,
    IJWTTokenService jwtTokenService,
    IHybridCache cache,
    IUnitOfWork unitOfWork,
    ILogger<GoogleLoginHandler> logger)
    : IRequestHandler<GoogleLoginCommand, Contract.Responses.Response<GoogleLoginResponse>>
{
    public async Task<Contract.Responses.Response<GoogleLoginResponse>> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.IdToken))
        {
            logger.LogWarning("GoogleLoginCommand received with empty IdToken.");
            return Contract.Responses.Response<GoogleLoginResponse>.Error("IdToken is required");
        }

        logger.LogInformation("Validating Google IdToken for request from IP: {IpAddress}", request.IpAddress);
        var googlePayload = await googleAuthService.ValidateIdTokenAsync(request.IdToken, cancellationToken);
        if (googlePayload == null)
        {
            logger.LogWarning("Invalid Google token received from IP: {IpAddress}", request.IpAddress);
            return Contract.Responses.Response<GoogleLoginResponse>.Error("Invalid Google token");
        }

        logger.LogInformation("Google token validated for subject: {Subject}, email: {Email}", googlePayload.Subject, googlePayload.Email);
        var user = await userRepository.GetByProviderAsync(ProviderConsts.GoogleProvider, googlePayload.Subject, cancellationToken);

        if (user == null)
        {
            logger.LogInformation("User not found by Google provider ID. Checking by email: {Email}", googlePayload.Email);
            user = await userRepository.GetByEmailAsync(googlePayload.Email, cancellationToken);
            
            if (user == null)
            {
                user = new UserCredential
                {
                    Id = Guid.NewGuid(),
                    Email = googlePayload.Email,
                    OAuthProvider = ProviderConsts.GoogleProvider,
                    ProviderId = googlePayload.Subject,
                    PasswordHash = string.Empty,
                    UserRoles = new HashSet<UserRole>()
                };

                logger.LogInformation("Creating new user for Google login: {Email}", user.Email);
                await userRepository.AddAsync(user, cancellationToken);
                
                await userManager.AddToRoleAsync(user, RoleConsts.User, cancellationToken);
                logger.LogInformation("New user {UserId} created and assigned 'User' role.", user.Id);
                
                // Create User entity directly (no need for event since Auth+User are now in one service)
                var userProfile = new Account.Domain.Entities.User
                {
                    Id = user.Id,
                    Email = user.Email,
                    Username = user.Email
                };
                
                logger.LogDebug("Creating User profile for user ID: {UserId}", user.Id);
                await userProfileRepository.AddAsync(userProfile);
                
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else
            {
                logger.LogInformation("Existing user found by email {Email}. Updating with Google provider details.", user.Email);
                user.OAuthProvider = ProviderConsts.GoogleProvider;
                user.ProviderId = googlePayload.Subject;
                await userRepository.UpdateAsync(user, cancellationToken);
            }

            logger.LogInformation("Saving changes to user {UserId} after Google login.", user.Id);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var authResult = await jwtTokenService.GenerateTokensAsync(
            user,
            request.DeviceName,
            request.IpAddress);

        logger.LogInformation("Tokens generated for user {UserId}.", user.Id);

        await CacheUser(user);
        logger.LogInformation("User {UserId} cached in Redis.", user.Id);

        var response = new GoogleLoginResponse
        {
            AccessToken = authResult.AccessToken,
            RefreshToken = authResult.RefreshToken,
            User = new Contract.Dtos.Auth.GoogleLoginUserDto
            {
                Email = user.Email,
                Username = user.Email,
                AvatarUrl = googlePayload.Picture
            }
        };

        logger.LogInformation("Google login successful for user {UserId}.", user.Id);
        return Contract.Responses.Response<GoogleLoginResponse>.Success(response);
    }

    private async Task CacheUser(UserCredential user)
    {
        user.PasswordHash = string.Empty;
        
        await cache.SetAsync(CacheKey.Auth.Credentials(user.Id), user);
        await cache.SetAsync(CacheKey.Auth.CredentialsByEmail(user.Email), user);
    }
}