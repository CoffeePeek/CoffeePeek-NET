using CoffeePeek.Account.Application.Features.Auth.Login;
using CoffeePeek.Account.Application.Features.Auth.Logout;
using CoffeePeek.Account.Application.Features.Auth.OAuthLogin;
using CoffeePeek.Account.Application.Features.Auth.RefreshToken;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Shared.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using LoginRequest = CoffeePeek.Account.Application.Features.Auth.Login.LoginRequest;

namespace CoffeePeek.AccountService.Controllers;

[ApiController]
[Route("api/[controller]")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class TokensController(IMediator mediator) : ControllerBase
{
    [HttpPost] 
    [ProducesResponseType<Response<LoginResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Login user and get token")]
    public async Task<IActionResult> Create([FromBody] LoginRequest request) 
    {
        var deviceName = Request.Headers.UserAgent.ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var enriched = new LoginUserCommand(request.Email, request.Password, deviceName, ipAddress);
        
        var result = await mediator.Send(enriched);
        
        return Ok(result);
    }

    [HttpPost("google/login")]
    [ProducesResponseType<Response<GoogleLoginResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation("OAuth login with google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginCommand request)
    {
        var command = request with
        {
            DeviceName = Request.Headers.UserAgent.ToString(),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
        };
        
        var response = await mediator.Send(command);

        return Ok(response);
    }
    
    [HttpPut] 
    [ProducesResponseType<Response<RefreshTokenResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation("Refresh token")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var deviceName = Request.Headers.UserAgent.ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var command = new RefreshTokenCommand(User.GetUserIdOrThrow(), request.RefreshToken, deviceName, ipAddress);
        
        var response = await mediator.Send(command);
        
        return Ok(response);
    }

    [HttpDelete]
    [Authorize]
    [SwaggerOperation("Logout user and invalidate refresh token")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete()
    {
        var request = new LogoutCommand(User.GetUserIdOrThrow(), User.GetUserRefreshTokenOrThrow());

        await mediator.Send(request);
        
        return NoContent();
    }
}