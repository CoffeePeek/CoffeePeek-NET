using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Auth.Options;
using CoffeePeek.Shared.Domain.Interfaces.Persistance;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using Microsoft.Extensions.Options;

namespace CoffeePeek.Account.Application.Features.Auth.RefreshToken;

public class RefreshTokenHandler
{
    public async Task<Response<RefreshTokenResponse>> Handle(RefreshTokenCommand request, 
        IUserRepository repository,
        IJWTTokenService tokenService,
        IUnitOfWork unitOfWork,
        IOptions<JWTOptions> jwtOptions,
        CancellationToken ct)
    {
        var user = await repository.GetById(request.UserId, ct);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        var newRefreshTokenValue = tokenService.GenerateRefreshToken();

        user.RotateRefreshToken(
            request.RefreshToken,
            newRefreshTokenValue,
            ttl: TimeSpan.FromDays(jwtOptions.Value.RefreshTokenLifetimeDays),
            request.DeviceName,
            request.IpAddress);
        
        var newAccessToken = tokenService.GenerateAccessToken(user);

        await unitOfWork.SaveChangesAsync(ct);

        return Response<RefreshTokenResponse>.Success(new RefreshTokenResponse(newAccessToken, newRefreshTokenValue));
    }
}