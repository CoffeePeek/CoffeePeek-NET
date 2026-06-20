using CoffeePeek.Account.Application.Features.Admin.Cache;
using CoffeePeek.Shared.Auth.Constants;
using CoffeePeek.Shared.Kernel.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CoffeePeek.AccountService.Controllers;

[ApiController]
[Route("api/admin/cache")]
[Authorize(Policy = RoleConsts.Admin)]
[Tags("Admin")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class AdminCacheController(IMessageBus bus) : ControllerBase
{
    [HttpGet("keys")]
    [ProducesResponseType<Response<GetAdminCacheKeysResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetKeys(
        [FromQuery] GetAdminCacheKeysQuery query,
        CancellationToken ct)
    {
        var response = await bus.InvokeAsync<Response<GetAdminCacheKeysResponse>>(query, ct);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPost("clear")]
    [ProducesResponseType<Response<ClearAdminCacheResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearByPattern(
        [FromBody] ClearAdminCacheByPatternCommand command,
        CancellationToken ct)
    {
        var response = await bus.InvokeAsync<Response<ClearAdminCacheResponse>>(command, ct);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPost("clear/{key}")]
    [ProducesResponseType<Response>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearKey(string key, CancellationToken ct)
    {
        var response = await bus.InvokeAsync<Response>(new ClearAdminCacheKeyCommand(key), ct);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }
}
