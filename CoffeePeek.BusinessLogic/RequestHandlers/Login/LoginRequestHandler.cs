using CoffeePeek.Contract.Requests.Auth;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.Login;
using CoffeePeek.Domain.Entities.Users;
using CoffeePeek.Infrastructure.Auth;
using CoffeePeek.Infrastructure.Cache.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CoffeePeek.BusinessLogic.RequestHandlers.Login;

public class LoginRequestHandler(
    IJWTTokenService jwtTokenService,
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    IRedisService redisService)
    : IRequestHandler<LoginRequest, Response<LoginResponse>>
{
    public async Task<Response<LoginResponse>> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await redisService.GetAsync<User>(request.Email);

        if (user == null)
        {
            try
            {
                user = await userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return Response.ErrorResponse<Response<LoginResponse>>("Account does not exist.");
                }
                
            }
            catch (Exception e)
            {
                return Response.ErrorResponse<Response<LoginResponse>>(e.Message);
            }
        }
        
        var signInResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!signInResult.Succeeded)
        {
            return Response.ErrorResponse<Response<LoginResponse>>("Password is incorrect.");
        }
        
        var authResult = await jwtTokenService.GenerateTokensAsync(user);

        await redisService.SetAsync($"{nameof(User)}{user.Id}", user);

        var result = new LoginResponse(authResult.AccessToken, authResult.RefreshToken);
        return Response.SuccessResponse<Response<LoginResponse>>(result);
    }
}
