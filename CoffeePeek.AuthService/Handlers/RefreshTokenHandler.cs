using CoffeePeek.AuthService.Commands;
using CoffeePeek.AuthService.Services;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.Auth;
using MediatR;

namespace CoffeePeek.AuthService.Handlers;

public class RefreshTokenHandler(IJWTTokenService jwtTokenService) : IRequestHandler<RefreshTokenCommand, Response<GetRefreshTokenResponse>>
{
    public async Task<Response<GetRefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        GetRefreshTokenResponse response;
        try
        {
            var authResult = await jwtTokenService.RefreshTokensAsync(request.RefreshToken, request.UserId);

            response = new GetRefreshTokenResponse(authResult.AccessToken, authResult.RefreshToken);
        }
        catch (UnauthorizedAccessException e)
        {
            return Response<GetRefreshTokenResponse>.Error("Invalid refresh token");
        }
        catch (Exception e)
        {
            return Response<GetRefreshTokenResponse>.Error("Error occurred");
        }

        return Response<GetRefreshTokenResponse>.Success(response);
    }
}