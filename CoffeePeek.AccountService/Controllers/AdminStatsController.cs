using CoffeePeek.Account.Application.Features.Admin.Cache;
using CoffeePeek.Account.Application.Features.Admin.Stats;
using CoffeePeek.Account.Application.Features.Admin.Users;
using CoffeePeek.Contract.Dtos.Admin;
using CoffeePeek.Shared.Auth.Constants;
using CoffeePeek.Shared.Kernel.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CoffeePeek.AccountService.Controllers;

[ApiController]
[Route("api/admin/stats")]
[Authorize(Policy = RoleConsts.Admin)]
[Tags("Admin")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class AdminStatsController(IMessageBus bus) : ControllerBase
{
    [HttpGet("overview")]
    [ProducesResponseType<Response<AdminOverviewStatsDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverview(CancellationToken ct)
    {
        var response = await bus.InvokeAsync<Response<AdminOverviewStatsDto>>(new GetAdminOverviewStatsQuery(), ct);
        return Ok(response);
    }
}
