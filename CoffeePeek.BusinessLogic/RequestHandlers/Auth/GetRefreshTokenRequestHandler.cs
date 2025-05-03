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
    IAuthService authService,
    IUnitOfWork<CoffeePeekDbContext> unitOfWork,
    IHashingService hashingService, IOptions<JWTOptions> jwtOptions)
    : IRequestHandler<GetRefreshTokenRequest, Response<GetRefreshTokenResponse>>
{
    private readonly JWTOptions _authOptions = jwtOptions.Value;
    public async Task<Response<GetRefreshTokenResponse>> Handle(GetRefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var decryptedRefreshTokenUserId = authService.DecryptRefreshToken(request.RefreshToken);

        if (decryptedRefreshTokenUserId is null)
        {
            throw new ArgumentNullException(nameof(decryptedRefreshTokenUserId), "Invalid refresh token");
        }
        
        var refreshTokenRecords = await GetRefreshTokenRecords(decryptedRefreshTokenUserId.Value, cancellationToken);
        if (refreshTokenRecords.Count == 0)
        {
            throw new UnauthorizedAccessException("RefreshToken does not exist");
        }

        foreach (var refreshTokenRecord in refreshTokenRecords)
        {
            if (refreshTokenRecord.ExpiryDate < DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("RefreshToken expired");
            }
        }

        var newRefreshToken = authService.GenerateRefreshToken(decryptedRefreshTokenUserId.Value);
        await UpdateRefreshTokens(refreshTokenRecords, newRefreshToken);

        var user = refreshTokenRecords.First().User;
        var accessToken = await authService.GenerateToken(user);

        var result = new GetRefreshTokenResponse(accessToken, newRefreshToken);

        return Response.SuccessResponse<Response<GetRefreshTokenResponse>>(result);
    }

    private async Task<List<RefreshToken>> GetRefreshTokenRecords(int userId, CancellationToken cancellationToken)
    {
        return await unitOfWork.DbContext.RefreshTokens
            .Include(x => x.User)
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    private async Task UpdateRefreshTokens(List<RefreshToken> refreshTokenRecords, string newRefreshToken)
    {
        foreach (var refreshTokenRecord in refreshTokenRecords)
        {
            refreshTokenRecord.Token = hashingService.HashString(newRefreshToken);
            refreshTokenRecord.ExpiryDate = DateTime.UtcNow.AddDays(_authOptions.RefreshTokenLifetimeDays);
        }

        await unitOfWork.SaveChangesAsync(CancellationToken.None);
    }
}
