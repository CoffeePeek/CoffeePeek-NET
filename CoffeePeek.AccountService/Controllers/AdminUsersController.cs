using CoffeePeek.Account.Application.Features.Admin.Users;
using CoffeePeek.Shared.Auth.Constants;
using CoffeePeek.Shared.Kernel.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CoffeePeek.AccountService.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = RoleConsts.Admin)]
[Tags("Admin")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class AdminUsersController(IMessageBus bus) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<Response<GetAdminUsersResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] GetAdminUsersQuery query,
        CancellationToken ct)
    {
        var response = await bus.InvokeAsync<Response<GetAdminUsersResponse>>(query, ct);

        if (response.IsSuccess && response.Data is not null)
        {
            Response.Headers.TryAdd("X-Total-Count", response.Data.TotalItems.ToString());
            Response.Headers.TryAdd("X-Total-Pages", response.Data.TotalPages.ToString());
            Response.Headers.TryAdd("X-Current-Page", response.Data.CurrentPage.ToString());
            Response.Headers.TryAdd("X-Page-Size", response.Data.PageSize.ToString());
        }

        return Ok(response);
    }

    [HttpGet("stats")]
    [ProducesResponseType<Response<GetAdminUsersStatsResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var response = await bus.InvokeAsync<Response<GetAdminUsersStatsResponse>>(new GetAdminUsersStatsQuery(), ct);
        return Ok(response);
    }

    [HttpPatch("{id:guid}/role")]
    [ProducesResponseType<Response<AdminUserResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateUserRoleRequest request, CancellationToken ct)
    {
        var command = new UpdateUserRoleCommand(id, request.Role);
        var response = await bus.InvokeAsync<Response<AdminUserResponse>>(command, ct);
        return response.IsSuccess ? Ok(response) : NotFound(response);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType<Response<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken ct)
    {
        var response = await bus.InvokeAsync<Response<bool>>(new AdminDeleteUserCommand(id), ct);
        return response.IsSuccess ? Ok(response) : NotFound(response);
    }
}

public record UpdateUserRoleRequest(string Role);
