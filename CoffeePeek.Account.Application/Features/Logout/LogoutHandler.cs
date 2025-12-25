using CoffeePeek.Account.Domain.Repositories;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Account.Application.Features.Logout;

public class LogoutHandler(
    IUserCredentialsRepository userRepository,
    IUnitOfWork unitOfWork,
    ILogger<LogoutHandler> logger) : IRequestHandler<LogoutCommand, Response>
{
    public async Task<Response> Handle(LogoutCommand request, CancellationToken ct)
    {
        try
        {
            logger.LogInformation("Attempting to log out user with ID: {UserId}", request.UserId);

            var user = await userRepository.GetByIdWithTokens(request.UserId, ct);
            
            if (user == null)
            {
                logger.LogWarning("Logout failed: User {UserId} not found.", request.UserId);
                return Response.Error("User not found");
            }

            user.Logout(request.RefreshToken);

            await unitOfWork.SaveChangesAsync(ct);

            logger.LogInformation("User {UserId} logged out successfully.", request.UserId);
            return Response.Success("Logout successful");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error during logout for user {UserId}", request.UserId);
            return Response.Error("An error occurred during logout");
        }
    }
}
