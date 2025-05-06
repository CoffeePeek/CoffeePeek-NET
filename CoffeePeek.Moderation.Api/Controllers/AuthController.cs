using CoffeePeek.Moderation.Application.Requests;
using CoffeePeek.Moderation.Application.Responses;
using CoffeePeek.Moderation.Contract.Abstract;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LoginRequest = CoffeePeek.Moderation.Application.Requests.LoginRequest;

namespace CoffeePeek.Moderation.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(IMediator mediator) : Controller
{
    [HttpPost("login")]
    public Task<Response<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        return mediator.Send(request);
    }

    [Authorize]
    [HttpPost("register")]
    public Task<Response<RegisterUserResponse>> Register([FromBody] RegisterUserRequest request)
    {
        return mediator.Send(request);
    }
    
    [HttpGet("refresh")]
    public Task<Response<GetRefreshTokenResponse>> RefreshToken([FromQuery] string refreshToken)
    {
        var request =  new GetRefreshTokenRequest(refreshToken);
        return mediator.Send(request);
    }
}