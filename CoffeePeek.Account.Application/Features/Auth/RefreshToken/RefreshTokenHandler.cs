using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Account.Application.Features.RefreshToken;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.Auth;
using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Options;
using MediatR;
using Microsoft.Extensions.Options;

namespace CoffeePeek.Account.Application.Features.Auth.RefreshToken;

public class RefreshTokenHandler(
    IUserRepository repository,
    IJWTTokenService tokenService,
    IUnitOfWork unitOfWork,
    IOptions<JWTOptions> jwtOptions) : IRequestHandler<RefreshTokenCommand, Response<GetRefreshTokenResponse>>
{
    public async Task<Response<GetRefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var user = await repository.GetById(request.UserId, ct);
        if (user == null) return Response<GetRefreshTokenResponse>.Error("User not found");

        try 
        {
            var newRefreshTokenValue = tokenService.GenerateRefreshToken();
            
            user.RotateRefreshToken(
                request.RefreshToken, 
                newRefreshTokenValue, 
                ttl: TimeSpan.FromDays(jwtOptions.Value.RefreshTokenLifetimeDays), 
                request.DeviceName, 
                request.IpAddress);

            var accessToken = tokenService.GenerateAccessToken(user, request.DeviceName, request.IpAddress);

            await unitOfWork.SaveChangesAsync(ct);

            return Response<GetRefreshTokenResponse>.Success(new GetRefreshTokenResponse(accessToken, newRefreshTokenValue));
        }
        catch (DomainException ex)
        {
            await unitOfWork.SaveChangesAsync(ct); 
            return Response<GetRefreshTokenResponse>.Error(ex.Message);
        }
    }
}