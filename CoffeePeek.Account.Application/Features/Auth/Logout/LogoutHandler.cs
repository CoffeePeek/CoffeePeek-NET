using CoffeePeek.Account.Application.Features.Logout;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Account.Application.Features.Auth.Logout;

public class LogoutHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ILogger<LogoutHandler> logger) : IRequestHandler<LogoutCommand, Response>
{
    public async Task<Response> Handle(LogoutCommand request, CancellationToken ct)
    {
        logger.LogInformation("Attempting to log out user with ID: {UserId}", request.UserId);

        var user = await userRepository.GetById(request.UserId, ct);

        if (user == null)
        {
            logger.LogWarning("Logout failed: User {UserId} not found.", request.UserId);
            return Response.Error(StatusCodes.Status404NotFound, "User not found");
        }

        user.Logout(request.RefreshToken);

        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("User {UserId} logged out successfully.", request.UserId);
        return Response.Success("Logout successful");
    }
}