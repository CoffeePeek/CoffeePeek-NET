using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Account.Application.Features.Auth.OAuthLogin;
using CoffeePeek.Account.Domain.Services;
using CoffeePeek.Contract.Dtos.Auth;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.Auth;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Options;
using MediatR;
using Microsoft.Extensions.Options;

namespace CoffeePeek.Account.Application.Features.OAuthLogin;

public class GoogleLoginHandler(
    IGoogleAuthService googleAuthService,
    IExternalAuthService externalAuthService,
    IJWTTokenService tokenService,
    IUnitOfWork unitOfWork,
    IHybridCache cache,
    IOptions<JWTOptions> options)
    : IRequestHandler<GoogleLoginCommand, Response<GoogleLoginResponse>>
{
    public async Task<Response<GoogleLoginResponse>> Handle(GoogleLoginCommand request, CancellationToken ct)
    {
        var payload = await googleAuthService.ValidateIdTokenAsync(request.IdToken);
        if (payload == null) return Response<GoogleLoginResponse>.Error("Invalid token");

        var user = await externalAuthService.GetOrCreate(
            payload.Email,
            ProviderConsts.GoogleProvider,
            payload.Subject,
            ct);

        var accessToken = tokenService.GenerateAccessToken(user.UserCredential, request.DeviceName, request.IpAddress);
        var refreshToken = tokenService.GenerateRefreshToken();

        var authResult = new AuthResult() { AccessToken = accessToken, RefreshToken = refreshToken };

        user.UserCredential.AddSession(
            authResult.RefreshToken,
            ttl: TimeSpan.FromDays(options.Value.AccessTokenLifetimeMinutes),
            request.DeviceName,
            request.IpAddress);

        await unitOfWork.SaveChangesAsync(ct);

        await cache.SetAsync(CacheKey.Auth.Credentials(user.UserCredential.Id), user.UserCredential,
            cancellationToken: ct);

        return Response<GoogleLoginResponse>.Success(new GoogleLoginResponse
        {
            AccessToken = authResult.AccessToken,
            RefreshToken = authResult.RefreshToken,
            User = new GoogleLoginUserDto { Email = user.UserCredential.Email, AvatarUrl = payload.Picture }
        });
    }
}