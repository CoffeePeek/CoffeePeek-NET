using CoffeePeek.Account.Application.Features.Admin.Cache;
using CoffeePeek.Shared.Auth.Constants;
using CoffeePeek.Shared.Kernel.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CoffeePeek.AccountService.Controllers;

/// <summary>Admin Redis cache management endpoints.</summary>
[ApiController]
[Route("api/admin/cache")]
[Authorize(Policy = RoleConsts.Admin)]
[Tags("Admin")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class AdminCacheController(IMessageBus bus) : ControllerBase
{
    /// <summary>Returns Redis keys matching the given pattern (SCAN-based, non-blocking).</summary>
    [HttpGet("keys")]
    [ProducesResponseType<Response<GetAdminCacheKeysResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetKeys(
        [FromQuery] GetAdminCacheKeysQuery query,
        CancellationToken cancellationToken)
    {
        var response = await bus.InvokeAsync<Response<GetAdminCacheKeysResponse>>(query, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    /// <summary>Deletes all keys matching the given pattern. Wildcard-only patterns are forbidden.</summary>
    [HttpPost("clear")]
    [ProducesResponseType<Response<ClearAdminCacheResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearByPattern(
        [FromBody] ClearAdminCacheByPatternCommand command,
        CancellationToken cancellationToken)
    {
        var response = await bus.InvokeAsync<Response<ClearAdminCacheResponse>>(command, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    /// <summary>Deletes a single Redis key by exact name.</summary>
    [HttpPost("clear/{key}")]
    [ProducesResponseType<Response>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearKey(string key, CancellationToken cancellationToken)
    {
        var response = await bus.InvokeAsync<Response>(new ClearAdminCacheKeyCommand(key), cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }
}
