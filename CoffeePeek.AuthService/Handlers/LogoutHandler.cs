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
                return Response.Error("User not found");
            }

            await userManager.RemoveAuthenticationTokenAsync(
                user,
                TokenOptions.DefaultProvider,
                RefreshTokenName
            );

            return Response.Success(message: "Logout successful");
        }
        catch (Exception e)
        {
            return Response.Error(message: "Error occurred during logout");
        }
    }
}

