using CoffeePeek.Contract.Requests.Auth;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.Login;
using CoffeePeek.Domain.Entities.Auth;
using CoffeePeek.Domain.Entities.Users;
using CoffeePeek.Domain.UnitOfWork;
using CoffeePeek.Infrastructure.Auth;
using CoffeePeek.Infrastructure.Cache.Interfaces;
using CoffeePeek.Infrastructure.Services;
using CoffeePeek.Infrastructure.Services.User.Interfaces;
using MediatR;

namespace CoffeePeek.BusinessLogic.RequestHandlers.Login;

public class LoginRequestHandler(
    IHashingService hashingService,
    IRepository<RefreshToken> refreshTokenRepository,
    IJWTTokenService jwtTokenService,
    IUserManager userManager,
    IRedisService redisService)
    : IRequestHandler<LoginRequest, Response<LoginResponse>>
{
    public async Task<Response<LoginResponse>> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await redisService.GetAsync<User>(request.Email);

        if (user == null)
        {
            user = await userManager.FindByEmailAsync(request.Email);
            
            if (user == null)
            {
                return Response.ErrorResponse<Response<LoginResponse>>("Account does not exist.");
            }
        }
        
        var isPasswordValid = userManager.CheckPassword(user, request.Password);
        if (!isPasswordValid)
        {
            return Response.ErrorResponse<Response<LoginResponse>>("Password is incorrect.");
        }
        
        var authResult = await jwtTokenService.GenerateTokensAsync(user);

        await SaveRefreshTokenAsync(user.Id, authResult.RefreshToken);

        var result = new LoginResponse
        {
            AccessToken = authResult.AccessToken,
            RefreshToken = authResult.RefreshToken,
        };

        return Response.SuccessResponse<Response<LoginResponse>>(result);
    }


    private async Task SaveRefreshTokenAsync(int userId, string refreshToken)
    {
        var refreshTokenHash = hashingService.HashString(refreshToken);

        var refreshTokenEntity = new RefreshToken
        {
            UserId = userId,
            Token = refreshTokenHash,
            ExpiryDate = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
        };

        refreshTokenRepository.Add(refreshTokenEntity);
        await refreshTokenRepository.SaveChangesAsync(CancellationToken.None);
    }
}
