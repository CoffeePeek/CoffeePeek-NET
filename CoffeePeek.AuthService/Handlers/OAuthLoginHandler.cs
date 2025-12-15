using CoffeePeek.AuthService.Commands;
using CoffeePeek.AuthService.Entities;
using CoffeePeek.AuthService.Repositories;
using CoffeePeek.AuthService.Services;
using CoffeePeek.Contract.Events;
using CoffeePeek.Contract.Response.Auth;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.Shared.Infrastructure;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Constants;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using MassTransit;
using MediatR;
using Response = CoffeePeek.Contract.Responses.Response;
using IJWTTokenService = CoffeePeek.AuthService.Services.IJWTTokenService;

namespace CoffeePeek.AuthService.Handlers;

public class GoogleLoginHandler(
    IGoogleAuthService googleAuthService,
    IUserCredentialsRepository userRepository,
    IUserManager userManager,
    IJWTTokenService jwtTokenService,
    IRedisService redisService,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint, 
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
                user = new UserCredentials
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
                await unitOfWork.SaveChangesAsync(cancellationToken);
                
                await userManager.AddToRoleAsync(user, RoleConsts.User, cancellationToken);
                logger.LogInformation("New user {UserId} created and assigned 'User' role.", user.Id);
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

        await publishEndpoint.Publish(new UserLoggedInEvent(user.Id), cancellationToken);
        logger.LogInformation("UserLoggedInEvent published for user {UserId}.", user.Id);

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

    private async Task CacheUser(UserCredentials user)
    {
        user.PasswordHash = string.Empty;
        
        await redisService.SetAsync(CacheKey.Auth.Credentials(user.Id), user);
        await redisService.SetAsync(CacheKey.Auth.CredentialsByEmail(user.Email), user);
    }
}