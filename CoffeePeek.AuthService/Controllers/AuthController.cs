using CoffeePeek.AuthService.Commands;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.Auth;
using CoffeePeek.Contract.Response.Login;
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
        return mediator.Send(command);
    }

    [HttpPost("register")]
    public Task<Response<RegisterUserResponse>> Register([FromBody] RegisterUserCommand command)
    {
        return mediator.Send(command);
    }
    
    [HttpGet("refresh")]
    public Task<Response<GetRefreshTokenResponse>> RefreshToken([FromQuery]string refreshToken)
    {
        var request = new RefreshTokenCommand(refreshToken)
        {
            UserId = User.GetUserIdOrThrow()
        };
        
        return mediator.Send(request);
    }
    
    [HttpPost("google/login")]
    public Task<Response<GoogleLoginResponse>> GoogleLogin([FromBody] GoogleLoginCommand command)
    {
        return mediator.Send(command);
    }
}