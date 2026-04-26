using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Account.Domain.Services;
using CoffeePeek.Shared.Auth.Options;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Response;
using Microsoft.Extensions.Options;

namespace CoffeePeek.Account.Application.Features.Auth.OAuthLogin;

public static class GoogleLoginHandler
{
    public static async Task<Response<GoogleLoginResponse>> Handle(
        GoogleLoginCommand request,
        IGoogleAuthService googleAuthService,
        IExternalAuthService externalAuthService,
        IJWTTokenService tokenService,
        IOptions<JWTOptions> options,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var payload = await googleAuthService.ValidateIdTokenAsync(request.IdToken, ct);
        if (payload is null)
            return Response<GoogleLoginResponse>.Error("Invalid token");

        var user = await externalAuthService.GetOrCreate(
            payload.Email,
            ProviderConsts.GoogleProvider,
            providerId: payload.Subject,
            ct);

        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();

        user.AddSession(
            refreshToken,
            ttl: TimeSpan.FromDays(options.Value.AccessTokenLifetimeMinutes),
            request.DeviceName,
            request.IpAddress);

        await unitOfWork.SaveChangesAsync(ct);
        
        return Response<GoogleLoginResponse>.Success(new GoogleLoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = new GoogleLoginUser
            {
                Email = user.Credentials.Email,
                AvatarUrl = payload.Picture,
                Username = user.Username
            }
        });
    }
}