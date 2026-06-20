using CoffeePeek.Account.Application.Features.Admin.Users;
using CoffeePeek.AccountService.Controllers.Admin;
using CoffeePeek.Shared.Auth.Constants;
using CoffeePeek.Shared.Kernel.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CoffeePeek.AccountService.Controllers;

/// <summary>Admin user management endpoints.</summary>
[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = RoleConsts.Admin)]
[Tags("Admin")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class AdminUsersController(IMessageBus bus) : ControllerBase
{
    /// <summary>Returns a paginated list of users with optional search and role filter.</summary>
    [HttpGet]
    [ProducesResponseType<Response<GetAdminUsersResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] GetAdminUsersQuery query,
        CancellationToken cancellationToken)
    {
        var response = await bus.InvokeAsync<Response<GetAdminUsersResponse>>(query, cancellationToken);

        if (response.IsSuccess && response.Data is not null)
        {
            Response.Headers.TryAdd("X-Total-Count", response.Data.TotalItems.ToString());
            Response.Headers.TryAdd("X-Total-Pages", response.Data.TotalPages.ToString());
            Response.Headers.TryAdd("X-Current-Page", response.Data.CurrentPage.ToString());
            Response.Headers.TryAdd("X-Page-Size", response.Data.PageSize.ToString());
        }

        return Ok(response);
    }

    /// <summary>Returns aggregate user statistics for the admin dashboard.</summary>
    [HttpGet("stats")]
    [ProducesResponseType<Response<GetAdminUsersStatsResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
    {
        var response = await bus.InvokeAsync<Response<GetAdminUsersStatsResponse>>(
            new GetAdminUsersStatsQuery(), cancellationToken);
        return Ok(response);
    }

    /// <summary>Replaces the target user's role.</summary>
    [HttpPatch("{id:guid}/role")]
    [ProducesResponseType<Response<Guid>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateRole(
        Guid id,
        [FromBody] UpdateUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateUserRoleCommand(id, request.Role);
        var response = await bus.InvokeAsync<Response<Guid>>(command, cancellationToken);

        if (response.IsSuccess)
            return Ok(response);

        return response.StatusCode switch
        {
            StatusCodes.Status404NotFound => NotFound(response),
            _ => BadRequest(response)
        };
    }

    /// <summary>Soft-deletes a user and revokes all active sessions.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType<Response<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        var response = await bus.InvokeAsync<Response<bool>>(new AdminDeleteUserCommand(id), cancellationToken);
        return response.IsSuccess ? Ok(response) : NotFound(response);
    }
}
