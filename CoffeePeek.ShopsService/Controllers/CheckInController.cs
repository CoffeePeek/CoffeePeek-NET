using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.Shared.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.ShopsService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CheckInController(IMediator mediator) : Controller
{
    [HttpPost]
    [ProducesResponseType(typeof(Response<CreateCheckInResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<Response<CreateCheckInResponse>> CreateCheckIn([FromBody] CreateCheckInRequest request)
    {
        var command = request with { UserId = HttpContext.User.GetUserIdOrThrow() };
        return mediator.Send(command);
    }

    [HttpGet]
    [ProducesResponseType(typeof(Response<GetUserCheckInsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<Response<GetUserCheckInsResponse>> GetMyCheckIns(
        [FromHeader(Name = "X-Page-Number")] int pageNumber = 1,
        [FromHeader(Name = "X-Page-Size")] int pageSize = 10)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);

        var userId = User.GetUserIdOrThrow();
        var result = await mediator.Send(new GetUserCheckInsCommand(userId, pageNumber, pageSize));

        if (result.IsSuccess && result.Data is not null)
        {
            AddPaginationHeaders(result.Data.TotalItems, result.Data.TotalPages, result.Data.CurrentPage, result.Data.PageSize);
        }

        return result;
    }

    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(Response<GetUserCheckInsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<Response<GetUserCheckInsResponse>> GetUserCheckIns(
        Guid userId,
        [FromHeader(Name = "X-Page-Number")] int pageNumber = 1,
        [FromHeader(Name = "X-Page-Size")] int pageSize = 10)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);

        var result = await mediator.Send(new GetUserCheckInsCommand(userId, pageNumber, pageSize));

        if (result.IsSuccess && result.Data is not null)
        {
            AddPaginationHeaders(result.Data.TotalItems, result.Data.TotalPages, result.Data.CurrentPage, result.Data.PageSize);
        }

        return result;
    }

    private void AddPaginationHeaders(int totalItems, int totalPages, int currentPage, int pageSize)
    {
        Response.Headers.TryAdd("X-Total-Count", totalItems.ToString());
        Response.Headers.TryAdd("X-Total-Pages", totalPages.ToString());
        Response.Headers.TryAdd("X-Current-Page", currentPage.ToString());
        Response.Headers.TryAdd("X-Page-Size", pageSize.ToString());
    }
}




