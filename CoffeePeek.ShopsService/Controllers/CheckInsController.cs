using CoffeePeek.Shared.Auth;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Application.Features.CheckIn.CreateCheckIn;
using CoffeePeek.Shops.Application.Features.CheckIn.GetUserCheckIns;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CoffeePeek.ShopsService.Controllers;

[ApiController]
[Route("api/[controller]")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class CheckInsController(IMessageBus bus, IUserContext userContext) : ControllerBase
{
    /// <summary>
    /// Create check-in for coffee shop
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(Response<CreateCheckInResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCheckIn([FromBody] CreateCheckInCommand command)
    {
        command = command with { UserId = userContext.GetUserIdOrThrow(), UserName = userContext.GetUsernameOrThrow() };
        var response = await bus.InvokeAsync<Response<CreateCheckInResponse>>(command);

        return Ok(response);
    }

    /// <summary>
    /// Get check-ins for current user
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Response<GetUserCheckInsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMyCheckIns(
        [FromHeader(Name = "X-Page-Number")] int pageNumber = 1,
        [FromHeader(Name = "X-Page-Size")] int pageSize = 10)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);

        var userId = userContext.GetUserIdOrThrow();
        
        var command = new GetUserCheckInsCommand(userId, pageNumber, pageSize);
        var response = await bus.InvokeAsync<Response<GetUserCheckInsResponse>>(command);

        if (response is { IsSuccess: true, Data: not null })
        {
            AddPaginationHeaders(response.Data.TotalItems, response.Data.TotalPages, response.Data.CurrentPage, response.Data.PageSize);
        }

        return Ok(response);
    }

    private void AddPaginationHeaders(int totalItems, int totalPages, int currentPage, int pageSize)
    {
        Response.Headers.TryAdd("X-Total-Count", totalItems.ToString());
        Response.Headers.TryAdd("X-Total-Pages", totalPages.ToString());
        Response.Headers.TryAdd("X-Current-Page", currentPage.ToString());
        Response.Headers.TryAdd("X-Page-Size", pageSize.ToString());
    }
}