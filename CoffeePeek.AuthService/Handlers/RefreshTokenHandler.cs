using CoffeePeek.AuthService.Commands;
using CoffeePeek.AuthService.Services;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.Auth;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Data.Interfaces;
using MediatR;

namespace CoffeePeek.AuthService.Handlers;

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