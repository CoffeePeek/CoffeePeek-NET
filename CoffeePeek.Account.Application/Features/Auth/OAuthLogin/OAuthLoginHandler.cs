using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Account.Application.Common.Models;
using CoffeePeek.Account.Domain.Services;
using CoffeePeek.Shared.Auth.Options;
using CoffeePeek.Shared.Domain.Interfaces.Persistance;
using CoffeePeek.Shared.Kernel.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Wolverine.Attributes;

namespace CoffeePeek.Account.Application.Features.Auth.OAuthLogin;

public static class GoogleLoginHandler
{
    [Transactional]
    public static async Task<Response<GoogleLoginResponse>> Handle(
        GoogleLoginCommand request,
        IGoogleAuthService googleAuthService,
        IExternalAuthService externalAuthService,
        IJWTTokenService tokenService,
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

        user.AddSession(
            refreshToken,
            ttl: TimeSpan.FromDays(options.Value.AccessTokenLifetimeMinutes),
            request.DeviceName,
            request.IpAddress);

        return Response<GoogleLoginResponse>.Success(new GoogleLoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = new GoogleLoginUser { Email = user.Credentials.Email, AvatarUrl = payload.Picture }
        });
    }
}