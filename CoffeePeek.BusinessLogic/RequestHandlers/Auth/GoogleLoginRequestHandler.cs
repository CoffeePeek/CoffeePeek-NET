using CoffeePeek.Contract.Dtos.Auth;
using CoffeePeek.Contract.Requests.Auth;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.Auth;
using CoffeePeek.Infrastructure.Auth;
using CoffeePeek.Infrastructure.Services;
using MediatR;

namespace CoffeePeek.BusinessLogic.RequestHandlers;

public class GoogleLoginRequestHandler(
    IGoogleAuthService googleAuthService,
    IUserService userService,
    IJWTTokenService jwtTokenService) 
    : IRequestHandler<GoogleLoginRequest, Response<GoogleLoginResponse>>
{
    public async Task<Response<GoogleLoginResponse>> Handle(GoogleLoginRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.IdToken))
            return Response.ErrorResponse<Response<GoogleLoginResponse>>("IdToken is required");

        var googlePayload = await googleAuthService.ValidateIdTokenAsync(request.IdToken);

        if (googlePayload == null)
            return Response.ErrorResponse<Response<GoogleLoginResponse>>("Invalid Google token");

        var user = await userService.GetOrCreateGoogleUserAsync(
            googleId: googlePayload.Subject,
            email: googlePayload.Email,
            name: googlePayload.Name,
            avatarUrl: googlePayload.Picture
        );

        var token = await jwtTokenService.GenerateTokensAsync(user);

        return Response.SuccessResponse<Response<GoogleLoginResponse>>(new GoogleLoginResponse
        {
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken,
            User = new GoogleLoginUserDto
            {
                Email = user.Email!,
                Username = user.UserName,
                AvatarUrl = user.AvatarUrl
            }
        });
    }
}