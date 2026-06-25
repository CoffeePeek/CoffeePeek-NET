using CoffeePeek.Account.Application.Features.CommunityNotifications;
using CoffeePeek.Shared.Auth;
using CoffeePeek.Shared.Kernel.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CoffeePeek.AccountService.Controllers;

/// <summary>Authenticated community notification inbox.</summary>
[ApiController]
[Authorize]
[Route("api/community/notifications")]
[Tags("Community")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class CommunityNotificationsController(IMessageBus bus, IUserContext userContext) : ControllerBase
{
    /// <summary>Returns paginated community notifications for the current user.</summary>
    [HttpGet]
    [ProducesResponseType<Response<GetCommunityNotificationsResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCommunityNotificationsQuery(page, pageSize)
        {
            UserId = userContext.GetUserIdOrThrow()
        };

        var response = await bus.InvokeAsync<Response<GetCommunityNotificationsResponse>>(query, cancellationToken);
        return Ok(response);
    }

    /// <summary>Marks a single notification as read.</summary>
    [HttpPatch("{notificationId:guid}/read")]
    [ProducesResponseType<Response>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkRead(Guid notificationId, CancellationToken cancellationToken)
    {
        var command = new MarkCommunityNotificationReadCommand(notificationId)
        {
            UserId = userContext.GetUserIdOrThrow()
        };

        var response = await bus.InvokeAsync<Response>(command, cancellationToken);
        return Ok(response);
    }

    /// <summary>Marks all notifications as read for the current user.</summary>
    [HttpPost("read-all")]
    [ProducesResponseType<Response>(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        var command = new MarkAllCommunityNotificationsReadCommand
        {
            UserId = userContext.GetUserIdOrThrow()
        };

        var response = await bus.InvokeAsync<Response>(command, cancellationToken);
        return Ok(response);
    }
}
