using CoffeePeek.AuthService.Commands;
using CoffeePeek.AuthService.Entities;
using CoffeePeek.AuthService.Models;
using CoffeePeek.AuthService.Services;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.Login;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SignInResult = CoffeePeek.AuthService.Models.SignInResult;

namespace CoffeePeek.AuthService.Handlers;

public class LoginUserHandler(
    IRedisService redisService,
    IUserManager userManager,
    IJWTTokenService jwtTokenService,
    ISignInManager signInManager)
    : IRequestHandler<LoginUserCommand, Response<LoginResponse>>
{
    public async Task<Response<LoginResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await redisService.GetAsync<UserCredentials>(request.Email);

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

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, request.Password);
        if (signInResult.Result != SignInResult.Success)
        {
            return Response.ErrorResponse<Response<LoginResponse>>("Password is incorrect.");
        }

        var authResult = await jwtTokenService.GenerateTokensAsync(user);

        await redisService.SetAsync($"{nameof(UserCredentials)}{user.Id}", user);

        var result = new LoginResponse(authResult.AccessToken, authResult.RefreshToken);
        return Response.SuccessResponse<Response<LoginResponse>>(result);
    }
}