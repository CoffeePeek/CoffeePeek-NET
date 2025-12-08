using CoffeePeek.AuthService.Commands;
using CoffeePeek.AuthService.Entities;
using CoffeePeek.AuthService.Repositories;
using CoffeePeek.AuthService.Services;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.Auth;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.Shared.Infrastructure;
using CoffeePeek.Shared.Infrastructure.Constants;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using MediatR;
using IJWTTokenService = CoffeePeek.AuthService.Services.IJWTTokenService;

namespace CoffeePeek.AuthService.Handlers;

public class GoogleLoginHandler(
    IGoogleAuthService googleAuthService,
    IUserCredentialsRepository userRepository,
    IUserManager userManager,
    IJWTTokenService jwtTokenService,
    IRedisService redisService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<GoogleLoginCommand, Response<GoogleLoginResponse>>
{
    public async Task<Response<GoogleLoginResponse>> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.IdToken))
        {
            return Response<GoogleLoginResponse>.Error("IdToken is required");
        }

        var googlePayload = await googleAuthService.ValidateIdTokenAsync(request.IdToken, cancellationToken);
        if (googlePayload == null)
        {
            return Response<GoogleLoginResponse>.Error("Invalid Google token");
        }

        var user = await userRepository.GetByProviderAsync(ProviderConsts.GoogleProvider, googlePayload.Subject, cancellationToken);

        if (user == null)
        {
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

                await userRepository.AddAsync(user, cancellationToken);
                // Сохраняем пользователя в БД перед добавлением роли, чтобы Id был установлен
                await unitOfWork.SaveChangesAsync(cancellationToken);
                
                await userManager.AddToRoleAsync(user, RoleConsts.User, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else
            {
                user.OAuthProvider = ProviderConsts.GoogleProvider;
                user.ProviderId = googlePayload.Subject;
                await userRepository.UpdateAsync(user, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        var authResult = await jwtTokenService.GenerateTokensAsync(user);

        await redisService.SetAsync($"{nameof(UserCredentials)}{user.Id}", user);

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

        return Response<GoogleLoginResponse>.Success(response);
    }
}