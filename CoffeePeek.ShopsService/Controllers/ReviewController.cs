using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Contract.Response.CoffeeShop.Review;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.ShopsService.Controllers;

[ApiController]
[Authorize(Policy = RoleConsts.User)]
[Route("api/[controller]")]
public class ReviewCoffeeController(IMediator mediator) : Controller
{
    [HttpGet]
    public async Task<Response<GetAllReviewsResponse>> GetAllReviews(
        [FromHeader(Name = "X-Page-Number")] int pageNumber = 1,
        [FromHeader(Name = "X-Page-Size")] int pageSize = 10)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize <= 0 ? 10 : pageSize, 1, 100);

        var result = await mediator.Send(new GetAllReviewsRequest(User.GetUserIdOrThrow(), pageNumber, pageSize));

        if (result.IsSuccess && result.Data is not null)
        {
            AddPaginationHeaders(result.Data.TotalItems, result.Data.TotalPages, result.Data.CurrentPage, result.Data.PageSize);
        }

        return result;
    }

    [HttpGet("{id:guid}")]
    public Task<Response<GetReviewByIdResponse>> GetReviewById(Guid id)
    {
        return mediator.Send(new GetReviewByIdCommand(id));
    }
    
    [HttpGet("user/{id:guid}")]
    public async Task<Response<GetReviewsByUserIdResponse>> GetReviewsByUserId(
        Guid id,
        [FromHeader(Name = "X-Page-Number")] int pageNumber = 1,
        [FromHeader(Name = "X-Page-Size")] int pageSize = 10)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var result = await mediator.Send(new GetReviewsByUserIdCommand(id, pageNumber, pageSize));

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

    [HttpPost]
    public Task<Response<AddCoffeeShopReviewResponse>> AddCoffeeShopReview([FromBody] AddCoffeeShopReviewRequest request)
    {
        var command = request with { UserId = User.GetUserIdOrThrow() };
        return mediator.Send(command);
    }

    [HttpPut]
    public Task<Response<UpdateCoffeeShopReviewResponse>> UpdateCoffeeShopReview([FromBody] UpdateCoffeeShopReviewRequest request)
    {
        request.UserId = User.GetUserIdOrThrow();
        return mediator.Send(request);
    }
}