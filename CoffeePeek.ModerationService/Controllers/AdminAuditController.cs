using CoffeePeek.Moderation.Application.Features.Admin.Audit;
using CoffeePeek.Shared.Auth.Constants;
using CoffeePeek.Shared.Kernel.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CoffeePeek.ModerationService.Controllers;

/// <summary>Admin moderation audit log endpoints.</summary>
[ApiController]
[Route("api/admin/audit")]
[Authorize(Policy = RoleConsts.Admin)]
[Tags("Admin")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class AdminAuditController(IMessageBus bus) : ControllerBase
{
    /// <summary>Returns a paginated moderation audit log.</summary>
    [HttpGet("moderation")]
    [ProducesResponseType<Response<GetModerationAuditLogResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetModerationAudit(
        [FromQuery] GetModerationAuditLogQuery query,
        CancellationToken cancellationToken)
    {
        var response = await bus.InvokeAsync<Response<GetModerationAuditLogResponse>>(query, cancellationToken);

        if (response.IsSuccess && response.Data is not null)
        {
            Response.Headers.TryAdd("X-Total-Count", response.Data.TotalItems.ToString());
            Response.Headers.TryAdd("X-Total-Pages", response.Data.TotalPages.ToString());
            Response.Headers.TryAdd("X-Current-Page", response.Data.CurrentPage.ToString());
            Response.Headers.TryAdd("X-Page-Size", response.Data.PageSize.ToString());
        }

        return Ok(response);
    }
}
