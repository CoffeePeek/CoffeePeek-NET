using CoffeePeek.Account.Application.Features.Auth.Login;
using CoffeePeek.Account.Application.Features.Auth.Logout;
using CoffeePeek.Account.Application.Features.Auth.OAuthLogin;
using CoffeePeek.Account.Application.Features.Auth.RefreshToken;
using CoffeePeek.Shared.Auth;
using CoffeePeek.Shared.Kernel.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CoffeePeek.AccountService.Controllers;

[ApiController]
[Route("api/[controller]")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class TokensController(IMessageBus bus, IUserContext userContext) : ControllerBase
{
    /// <summary>
    /// Login user and get token
    /// </summary>
    [HttpPost]
    [ProducesResponseType<Response<LoginResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] LoginUserCommand request)
    {
        var deviceName = Request.Headers.UserAgent.ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var command = request with { DeviceName = deviceName, IpAddress = ipAddress };

        var response = await bus.InvokeAsync<Response<LoginResponse>>(command);

        return Ok(response);
    }

    /// <summary>
    /// OAuth login with google
    /// </summary>
    [HttpPost("google/login")]
    [ProducesResponseType<Response<GoogleLoginResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginCommand request)
    {
        var command = request with
        {
            DeviceName = Request.Headers.UserAgent.ToString(),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
        };

        var response = await bus.InvokeAsync<Response<GoogleLoginResponse>>(command);

        return Ok(response);
    }

    /// <summary>
    /// Refresh token
    /// </summary>
    [HttpPut]
    [Authorize]
    [ProducesResponseType<Response<RefreshTokenResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command)
    {
        var deviceName = Request.Headers.UserAgent.ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        command = command with { UserId = userContext.GetUserIdOrThrow(), DeviceName = deviceName, IpAddress = ipAddress };

        var response = await bus.InvokeAsync<Response<RefreshTokenResponse>>(command);

        return Ok(response);
    }

    /// <summary>
    /// Logout user and invalidate refresh token from cookies
    /// </summary>
    [HttpDelete]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete()
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
        {
            return BadRequest(new { message = "Refresh token is required" });
        }

        var request = new LogoutCommand(userContext.GetUserIdOrThrow(), refreshToken);

        await bus.InvokeAsync(request);

        return NoContent();
    }
}