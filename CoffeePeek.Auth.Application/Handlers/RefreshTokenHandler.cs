using CoffeePeek.Auth.Application.Commands;
using CoffeePeek.Auth.Application.Services;
using CoffeePeek.Auth.Application.Services.Interfaces;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.Auth;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Auth.Application.Handlers;

public class RefreshTokenHandler(
    IJWTTokenService jwtTokenService,
    IUnitOfWork unitOfWork,
    ILogger<RefreshTokenHandler> logger) 
    : IRequestHandler<RefreshTokenCommand, Response<GetRefreshTokenResponse>>
{
    public async Task<Response<GetRefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        GetRefreshTokenResponse response;
        try
        {
            logger.LogInformation("Attempting to refresh tokens for user {UserId} with device {DeviceName}", request.UserId, request.DeviceName);

            var authResult = await jwtTokenService.RefreshTokensAsync(
                request.RefreshToken,
                request.UserId,
                request.DeviceName,
                request.IpAddress);
            
            await unitOfWork.SaveChangesAsync(cancellationToken);

            response = new GetRefreshTokenResponse(authResult.AccessToken, authResult.RefreshToken);
            logger.LogInformation("Tokens successfully refreshed for user {UserId}", request.UserId);
        }
        catch (UnauthorizedAccessException)
        {
            logger.LogWarning("Unauthorized access attempt during token refresh for user {UserId}", request.UserId);
            return Response<GetRefreshTokenResponse>.Error("Invalid refresh token");
        }
        catch (Exception)
        {
            logger.LogError("An unexpected error occurred during token refresh for user {UserId}", request.UserId);
            return Response<GetRefreshTokenResponse>.Error("Error occurred");
        }

        return Response<GetRefreshTokenResponse>.Success(response);
    }
}