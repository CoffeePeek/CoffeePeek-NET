using CoffeePeek.Contract.Requests.Auth;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.Api.Controllers;

[ApiController]
[Route("api/auth/google")]
public class GoogleAuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("login")]
    public Task<Response<GoogleLoginResponse>> Login([FromBody] GoogleLoginRequest request)
    {
        return mediator.Send(request);
    }
}