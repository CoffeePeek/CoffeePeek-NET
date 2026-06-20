using CoffeePeek.Contract.Dtos.Admin;
using CoffeePeek.Moderation.Application.Features.Admin.Stats;
using CoffeePeek.Shared.Auth.Constants;
using CoffeePeek.Shared.Kernel.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CoffeePeek.ModerationService.Controllers;

[ApiController]
[Route("api/admin/stats")]
[Authorize(Policy = RoleConsts.Admin)]
[Tags("Admin")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class AdminStatsController(IMessageBus bus) : ControllerBase
{
    [HttpGet("summary")]
    [ProducesResponseType<Response<AdminServiceStatsDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
    {
        var response = await bus.InvokeAsync<Response<AdminServiceStatsDto>>(new GetAdminModerationStatsQuery(), ct);
        return Ok(response);
    }
}
