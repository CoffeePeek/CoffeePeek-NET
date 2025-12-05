using CoffeePeek.AuthService.Commands;
using CoffeePeek.AuthService.Services;
using CoffeePeek.Contract.Response;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CoffeePeek.AuthService.Handlers;

public class LogoutHandler(IUserManager userManager) : IRequestHandler<LogoutCommand, Response>
{
    private const string RefreshTokenName = "RefreshToken";

    public async Task<Response> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return Response.ErrorResponse<Response>("User not found");
            }

            await userManager.RemoveAuthenticationTokenAsync(
                user,
                TokenOptions.DefaultProvider,
                RefreshTokenName
            );

            return Response.SuccessResponse<Response>(message: "Logout successful");
        }
        catch (Exception e)
        {
            return Response.ErrorResponse<Response>("Error occurred during logout");
        }
    }
}

