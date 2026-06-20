using CoffeePeek.Account.Application.Features.Admin.Stats;
using CoffeePeek.Contract.Dtos.Admin;
using CoffeePeek.Shared.Auth.Constants;
using CoffeePeek.Shared.Kernel.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CoffeePeek.AccountService.Controllers;

/// <summary>Admin dashboard statistics endpoints.</summary>
[ApiController]
[Route("api/admin/stats")]
[Authorize(Policy = RoleConsts.Admin)]
[Tags("Admin")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class AdminStatsController(IMessageBus bus) : ControllerBase
{
    /// <summary>Returns platform-wide overview statistics aggregated from all services.</summary>
    [HttpGet("overview")]
    [ProducesResponseType<Response<AdminOverviewStatsDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetOverview(CancellationToken cancellationToken)
    {
        var response = await bus.InvokeAsync<Response<AdminOverviewStatsDto>>(
            new GetAdminOverviewStatsQuery(), cancellationToken);

        if (!response.IsSuccess && response.StatusCode == StatusCodes.Status503ServiceUnavailable)
            return StatusCode(StatusCodes.Status503ServiceUnavailable, response);

        return Ok(response);
    }
}
