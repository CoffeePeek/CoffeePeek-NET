using CoffeePeek.AuthService.Commands;
using CoffeePeek.Contract.Response.Auth;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.Login;
using CoffeePeek.Shared.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.AuthService.Controllers;

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
    public Task<Response<LoginResponse>> Login([FromBody] LoginUserCommand command)
    {
        var deviceName = Request.Headers.UserAgent.ToString() ?? "unknown";
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var enriched = command with { DeviceName = deviceName, IpAddress = ipAddress };
        return mediator.Send(enriched);
    }

    [HttpPost("register")]
    public Task<CreateEntityResponse<Guid>> Register([FromBody] RegisterUserCommand command)
    {
        return mediator.Send(command);
    }
    
    [HttpGet("refresh")]
    public Task<Response<GetRefreshTokenResponse>> RefreshToken([FromQuery] string refreshToken)
    {
        var deviceName = Request.Headers.UserAgent.ToString() ?? "unknown";
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
        var deviceName = Request.Headers.UserAgent.ToString() ?? "unknown";
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var enriched = new GoogleLoginCommand(command.IdToken, deviceName, ipAddress);
        return mediator.Send(enriched);
    }
    
    [HttpPost("logout")]
    public Task<Response> Logout()
    {
        var request = new LogoutCommand
        {
            UserId = User.GetUserIdOrThrow()
        };
        
        return mediator.Send(request);
    }
}