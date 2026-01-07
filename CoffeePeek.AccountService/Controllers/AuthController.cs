using CoffeePeek.Account.Application.Commands;
using CoffeePeek.Account.Application.Features.Auth.CheckUserExistsByEmail;
using CoffeePeek.Account.Application.Features.Auth.Login;
using CoffeePeek.Account.Application.Features.Login;
using CoffeePeek.Account.Application.Features.Logout;
using CoffeePeek.Account.Application.Features.RefreshToken;
using CoffeePeek.Account.Application.Features.User.RegisterUser;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.Auth;
using CoffeePeek.Contract.Responses.Login;
using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.AccountService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpGet("check-exists")]
    public Task<Response<bool>> CheckUserExistsByEmail([FromQuery] string email)
    {
        var request = new CheckUserExistsByEmailCommand(email);
        return mediator.Send(request);
    }

    [HttpPost("login")]
    public async Task<Response<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var deviceName = Request.Headers.UserAgent.ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var enriched = new LoginUserCommand(request.Email, request.Password, deviceName, ipAddress);
        return await mediator.Send(enriched);
    }

    [HttpPost("register")]
    public Task<CreateEntityResponse> Register([FromBody] RegisterUserCommand command)
    {
        return mediator.Send(command);
    }

    [HttpPost("refresh")]
    public Task<Response<GetRefreshTokenResponse>> RefreshToken([FromQuery] string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
            throw new UnauthorizedException("Refresh token missing");
        
        var deviceName = Request.Headers.UserAgent.ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var request = new RefreshTokenCommand(refreshToken, deviceName, ipAddress)
        {
            UserId = User.GetUserIdOrThrow()
        };
        
        return mediator.Send(request);
    }

    [HttpPost("google/login")]
    public Task<Response<GoogleLoginResponse>> GoogleLogin([FromBody] GoogleLoginCommand command)
    {
        var deviceName = Request.Headers.UserAgent.ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var enriched = new GoogleLoginCommand(command.IdToken, deviceName, ipAddress);
        return mediator.Send(enriched);
    }

    [HttpPost("logout")]
    public Task<Response> Logout([FromQuery] string refreshToken)
    {
        var request = new LogoutCommand(User.GetUserIdOrThrow(), refreshToken);

        return mediator.Send(request);
    }
}