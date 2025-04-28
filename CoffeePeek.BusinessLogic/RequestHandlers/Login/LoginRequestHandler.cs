using CoffeePeek.Contract.Requests.Auth;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.Login;
using CoffeePeek.Data;
using CoffeePeek.Domain.Entities.Auth;
using CoffeePeek.Domain.Entities.Users;
using CoffeePeek.Domain.UnitOfWork;
using CoffeePeek.Infrastructure.Cache.Interfaces;
using CoffeePeek.Infrastructure.Services;
using CoffeePeek.Infrastructure.Services.Auth.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CoffeePeek.BusinessLogic.RequestHandlers.Login;

public class LoginRequestHandler(
    IHashingService hashingService,
    IRepository<RefreshToken> refreshTokenRepository,
    IAuthService authService,
    UserManager<User> userManager, 
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
        
        var isPasswordValid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            return Response.ErrorResponse<Response<LoginResponse>>("Password is incorrect.");
        }
        
        var accessToken = await authService.GenerateToken(user);
        var refreshToken = GenerateRefreshToken(user.Id);

        await SaveRefreshTokenAsync(user.Id, refreshToken);

        var result = new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };

        return Response.SuccessResponse<Response<LoginResponse>>(result);
    }

    private string GenerateRefreshToken(int userId)
    {
        var refreshToken = authService.GenerateRefreshToken(userId);
        return refreshToken;
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
