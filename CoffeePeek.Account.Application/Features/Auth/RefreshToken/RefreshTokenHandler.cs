using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Auth.Options;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using Microsoft.Extensions.Options;
using Wolverine.Attributes;

namespace CoffeePeek.Account.Application.Features.Auth.RefreshToken;

public class RefreshTokenHandler
{
    [Transactional]
    public static async Task<Response<RefreshTokenResponse>> Handle(
        RefreshTokenCommand request,
        IUserRepository repository,
        IJWTTokenService tokenService,
        IOptions<JWTOptions> jwtOptions,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var user = await repository.GetById(request.UserId, ct)
                   ?? throw new NotFoundException("User not found");

        var newRefreshTokenValue = tokenService.GenerateRefreshToken();
        var newAccessToken = tokenService.GenerateAccessToken(user);

        user.RotateRefreshToken(
            request.RefreshToken,
            newRefreshTokenValue,
            ttl: TimeSpan.FromDays(jwtOptions.Value.RefreshTokenLifetimeDays),
            request.DeviceName,
            request.IpAddress);

        await unitOfWork.SaveChangesAsync(ct);
        
        return Response<RefreshTokenResponse>.Success(new RefreshTokenResponse(newAccessToken, newRefreshTokenValue));
    }
}