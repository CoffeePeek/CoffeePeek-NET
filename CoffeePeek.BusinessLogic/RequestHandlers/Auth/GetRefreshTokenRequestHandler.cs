using CoffeePeek.Contract.Requests.Auth;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.Auth;
using CoffeePeek.Domain.Databases;
using CoffeePeek.Domain.Entities.Auth;
using CoffeePeek.Domain.UnitOfWork;
using CoffeePeek.Infrastructure.Auth;
using CoffeePeek.Infrastructure.Services;
using CoffeePeek.Infrastructure.Services.Auth.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CoffeePeek.BusinessLogic.RequestHandlers;

public class GetRefreshTokenRequestHandler(
    IJWTTokenService jwtTokenService)
    : IRequestHandler<GetRefreshTokenRequest, Response<GetRefreshTokenResponse>>
{
    public async Task<Response<GetRefreshTokenResponse>> Handle(GetRefreshTokenRequest request, CancellationToken cancellationToken)
    {
        GetRefreshTokenResponse response;
        try
        {
            var authResult = await jwtTokenService.RefreshTokensAsync(request.RefreshToken);

            response = new GetRefreshTokenResponse(authResult.AccessToken, authResult.RefreshToken);
        }
        catch (UnauthorizedAccessException e)
        {
            return Response.ErrorResponse<Response<GetRefreshTokenResponse>>("Invalid refresh token", e);
        }
        catch (Exception e)
        {
            return Response.ErrorResponse<Response<GetRefreshTokenResponse>>("Error occurred", e);
        }

        return Response.SuccessResponse<Response<GetRefreshTokenResponse>>(response);
    }
}
