using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Account.Application.Common.Models;
using CoffeePeek.Account.Domain.Services;
using CoffeePeek.Shared.Auth.Options;
using CoffeePeek.Shared.Domain.Interfaces.Persistance;
using CoffeePeek.Shared.Kernel.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace CoffeePeek.Account.Application.Features.Auth.OAuthLogin;

public class GoogleLoginHandler
{
    public static async Task<Response<GoogleLoginResponse>> Handle(GoogleLoginCommand request, 
        IGoogleAuthService googleAuthService,
        IExternalAuthService externalAuthService,
        IJWTTokenService tokenService,
        IUnitOfWork unitOfWork,
        IOptions<JWTOptions> options,
        CancellationToken ct)
    {
        var payload = await googleAuthService.ValidateIdTokenAsync(request.IdToken);
        if (payload == null)
        {
            throw new BadHttpRequestException("Invalid token");
        }

        var user = await externalAuthService.GetOrCreate(
            payload.Email,
            ProviderConsts.GoogleProvider,
            providerId: payload.Subject,
            ct);

        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();

        var authResult = new AuthResult { AccessToken = accessToken, RefreshToken = refreshToken };

        user.AddSession(
            authResult.RefreshToken,
            ttl: TimeSpan.FromDays(options.Value.AccessTokenLifetimeMinutes),
            request.DeviceName,
            request.IpAddress);

        await unitOfWork.SaveChangesAsync(ct);

        return Response<GoogleLoginResponse>.Success(new GoogleLoginResponse
        {
            AccessToken = authResult.AccessToken,
            RefreshToken = authResult.RefreshToken,
            User = new GoogleLoginUser { Email = user.Credentials.Email, AvatarUrl = payload.Picture }
        });
    }
}