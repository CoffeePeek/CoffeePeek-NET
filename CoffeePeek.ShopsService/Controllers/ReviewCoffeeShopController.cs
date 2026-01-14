using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.Shared.Infrastructure;
using CoffeePeek.Shops.Application.Features.CoffeeShop.CreateCoffeeShopReview;
using CoffeePeek.Shops.Application.Features.CoffeeShop.DeleteReviewFromCoffeeShop;
using CoffeePeek.Shops.Application.Features.Review.GetAllReviewsByShopId;
using CoffeePeek.Shops.Application.Features.Review.GetReviewById;
using CoffeePeek.Shops.Application.Features.Review.GetReviewsByUserId;
using CoffeePeek.Shops.Application.Features.Review.UpdateCoffeeShopReview;
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
        return mediator.Send(new GetReviewByIdQuery(reviewId));
    }
    
    [HttpGet("user/{id:guid}")]
    public async Task<Response<GetReviewsByUserIdResponse>> GetReviewsByUserId(
        Guid id,
        [FromHeader(Name = "X-Page-Number")] int pageNumber = 1,
        [FromHeader(Name = "X-Page-Size")] int pageSize = 10)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var result = await mediator.Send(new GetReviewsByUserIdQuery(id, pageNumber, pageSize));

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

    [HttpDelete("{reviewId:guid}")]
    public Task<Response> RemoveCoffeeShopReview(Guid reviewId)
    {
        return mediator.Send(new DeleteReviewFromCoffeeShopCommand(reviewId));
    }
    
    private void AddPaginationHeaders(int totalItems, int totalPages, int currentPage, int pageSize)
    {
        Response.Headers.TryAdd("X-Total-Count", totalItems.ToString());
        Response.Headers.TryAdd("X-Total-Pages", totalPages.ToString());
        Response.Headers.TryAdd("X-Current-Page", currentPage.ToString());
        Response.Headers.TryAdd("X-Page-Size", pageSize.ToString());
    }
}