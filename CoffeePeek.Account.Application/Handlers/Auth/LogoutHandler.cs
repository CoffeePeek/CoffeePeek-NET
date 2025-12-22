using CoffeePeek.Account.Application.Commands;
using CoffeePeek.Account.Application.Services;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Extensions.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Account.Application.Handlers;

public class LogoutHandler(IUserManager userManager, ILogger<LogoutHandler> logger) : IRequestHandler<LogoutCommand, Response>
{
    private const string RefreshTokenName = "RefreshToken";

    public async Task<Response> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userManager.FindByIdAsync(request.UserId);
            logger.LogInformation("Attempting to log out user with ID: {UserId}", request.UserId);
            if (user == null)
            {
                logger.LogWarning("Logout failed: User with ID {UserId} not found.", request.UserId);
                throw new NotFoundException("User not found");
            }

            logger.LogInformation("Removing refresh token for user with ID: {UserId}", request.UserId);
            await userManager.RemoveAuthenticationTokenAsync(
                user,
                TokenOptions.DefaultProvider,
                RefreshTokenName
            );

            return Response.Success(message: "Logout successful");
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred during logout for user with ID: {UserId}", request.UserId);
            return Response.Error(message: "Error occurred during logout");
        }
    }
}
