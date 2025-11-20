using CoffeePeek.Contract.Requests.Auth;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.Auth;
using CoffeePeek.Contract.Response.Login;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IMediator mediator): Controller
{
    [HttpPost("check-exists")]
    public Task<Response> CheckUserExistsByEmail([FromQuery] string email)
    {
        var request = new CheckUserExistsByEmailRequest(email);
        return mediator.Send(request);
    }
    
    [HttpPost("login")]
    public Task<Response<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        return mediator.Send(request);
    }

    [HttpPost("register")]
    public Task<Response<RegisterUserResponse>> Register([FromBody] RegisterUserRequest request)
    {
        return mediator.Send(request);
    }

    [Obsolete, HttpPost("register-google")]
    public Task<string> Register([FromBody] RegisterUserFromGoogleRequest request)
    {
        return Task.FromResult("not working");
        //return mediator.Send(request);
    }
    
    [HttpGet("refresh")]
    public Task<Response<GetRefreshTokenResponse>> RefreshToken([FromQuery]string refreshToken)
    {
        var request =  new GetRefreshTokenRequest(refreshToken);
        return mediator.Send(request);
    }
}