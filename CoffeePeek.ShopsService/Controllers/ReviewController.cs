using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Contract.Response.CoffeeShop.Review;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.Contract.Responses.CoffeeShop.Review;
using CoffeePeek.Shared.Infrastructure;
using CoffeePeek.Shops.Application.Commands.CoffeeShop;
using CoffeePeek.Shops.Application.Commands.CoffeeShop.Review;
using CoffeePeek.Shops.Application.Features.CoffeeShop.CreateCoffeeShopReview;
using CoffeePeek.Shops.Application.Features.CoffeeShop.DeleteReviewFromCoffeeShop;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.ShopsService.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ReviewCoffeeShopController(IMediator mediator) : Controller
{
    [HttpGet]
    public async Task<Response<GetAllReviewsResponse>> GetAllReviewsByShopId(
        [FromQuery] Guid shopId,
        [FromHeader(Name = "X-Page-Size")] int pageSize = 10,
        [FromHeader(Name = "X-Page-Number")] int pageNumber = 1)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize <= 0 ? 10 : pageSize, 1, 100);

        var response = await mediator.Send(new GetAllReviewsByShopIdQuery(shopId, pageNumber, pageSize));

        if (response is { IsSuccess: true, Data: not null })
        {
            AddPaginationHeaders(response.Data.TotalItems, response.Data.TotalPages, response.Data.CurrentPage,
                response.Data.PageSize);
        }

        return response;
    }

    [HttpGet("{reviewId:guid}")]
    public Task<Response<GetReviewByIdResponse>> GetReviewById(Guid reviewId)
    {
        return mediator.Send(new GetReviewByIdCommand(reviewId));
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

        if (result is { IsSuccess: true, Data: not null })
        {
            AddPaginationHeaders(result.Data.TotalItems, result.Data.TotalPages, result.Data.CurrentPage, result.Data.PageSize);
        }

        return result;
    }

    [HttpPost]
    public Task<Response<CreateCoffeeShopReviewResponse>> AddCoffeeShopReview([FromBody] CreateCoffeeShopReviewCommand command)
    {
        command.UserId = User.GetUserIdOrThrow();
        return mediator.Send(command);
    }

    [HttpPut]
    public Task<Response<UpdateCoffeeShopReviewResponse>> UpdateCoffeeShopReview([FromBody] UpdateCoffeeShopReviewRequest request)
    {
        request.UserId = User.GetUserIdOrThrow();
        return mediator.Send(request);
    }

    [HttpDelete("{id:guid}")]
    public Task<Response> RemoveCoffeeShopReview(Guid id)
    {
        return mediator.Send(new DeleteReviewFromCoffeeShopCommand(id));
    }
    
    private void AddPaginationHeaders(int totalItems, int totalPages, int currentPage, int pageSize)
    {
        Response.Headers.TryAdd("X-Total-Count", totalItems.ToString());
        Response.Headers.TryAdd("X-Total-Pages", totalPages.ToString());
        Response.Headers.TryAdd("X-Current-Page", currentPage.ToString());
        Response.Headers.TryAdd("X-Page-Size", pageSize.ToString());
    }
}