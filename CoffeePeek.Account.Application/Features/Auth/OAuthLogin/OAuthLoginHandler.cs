using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Account.Application.Common.Models;
using CoffeePeek.Account.Domain.Services;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Options;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace CoffeePeek.Account.Application.Features.Auth.OAuthLogin;

public class GoogleLoginHandler(
    IGoogleAuthService googleAuthService,
    IExternalAuthService externalAuthService,
    IJWTTokenService tokenService,
    IUnitOfWork unitOfWork,
    IRedisService redisService,
    IOptions<JWTOptions> options)
    : IRequestHandler<GoogleLoginCommand, Response<GoogleLoginResponse>>
{
    public async Task<Response<GoogleLoginResponse>> Handle(GoogleLoginCommand request, CancellationToken ct)
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

        var accessToken = tokenService.GenerateAccessToken(user, request.DeviceName, request.IpAddress);
        var refreshToken = tokenService.GenerateRefreshToken();

        var authResult = new AuthResult { AccessToken = accessToken, RefreshToken = refreshToken };

        user.AddSession(
            authResult.RefreshToken,
            ttl: TimeSpan.FromDays(options.Value.AccessTokenLifetimeMinutes),
            request.DeviceName,
            request.IpAddress);

        await unitOfWork.SaveChangesAsync(ct);

        await redisService.SetAsync(CacheKey.Auth.Credentials(user.Id), user);

        return Response<GoogleLoginResponse>.Success(new GoogleLoginResponse
        {
            AccessToken = authResult.AccessToken,
            RefreshToken = authResult.RefreshToken,
            User = new GoogleLoginUser { Email = user.Credentials.Email, AvatarUrl = payload.Picture }
        });
    }
}