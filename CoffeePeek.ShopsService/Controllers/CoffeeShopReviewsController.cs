using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.Shared.Infrastructure;
using CoffeePeek.Shops.Application.Features.CoffeeShop.CreateCoffeeShopReview;
using CoffeePeek.Shops.Application.Features.CoffeeShop.DeleteReviewFromCoffeeShop;
using CoffeePeek.Shops.Application.Features.Review.CanCreateCoffeeShopReview;
using CoffeePeek.Shops.Application.Features.Review.GetAllReviewsByShopId;
using CoffeePeek.Shops.Application.Features.Review.GetReviewById;
using CoffeePeek.Shops.Application.Features.Review.UpdateCoffeeShopReview;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.ShopsService.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CoffeeShopReviewsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(Response<GetAllReviewsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReviews(
        [FromQuery] Guid shopId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] [Range(1, 100)] int pageSize = 10)
    {
        if (shopId == Guid.Empty)
            return BadRequest(Response<GetAllReviewsResponse>.Error("Invalid shop ID"));

        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = new GetAllReviewsByShopIdQuery(shopId, pageNumber, pageSize);
        var response = await mediator.Send(query);

        if (response is { IsSuccess: true, Data: not null })
        {
            AddPaginationHeaders(response.Data);
        }

        return Ok(response);
    }

    [HttpGet("{reviewId:guid}")]
    [ProducesResponseType(typeof(Response<GetReviewByIdResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReview(Guid shopId, Guid reviewId)
    {
        var query = new GetReviewByIdQuery(reviewId);
        var response = await mediator.Send(query);

        return response.IsSuccess ? Ok(response) : NotFound(response);
    }

    [HttpGet("can-create")]
    [ProducesResponseType(typeof(Response<CanCreateCoffeeShopReviewResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CanCreateReview([FromQuery] Guid shopId)
    {
        if (shopId == Guid.Empty)
            return BadRequest(Response<CanCreateCoffeeShopReviewResponse>.Error("Invalid shop ID"));

        var userId = User.GetUserIdOrThrow();
        var query = new CanCreateCoffeeShopReviewQuery(userId, shopId);
        var response = await mediator.Send(query);

        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Response<CreateCoffeeShopReviewResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateReview(
        Guid shopId,
        [FromBody] CreateCoffeeShopReviewCommand command)
    {
        if (shopId == Guid.Empty)
            return BadRequest(Response<CreateCoffeeShopReviewResponse>.Error("Invalid shop ID"));

        command.UserId = User.GetUserIdOrThrow();

        var response = await mediator.Send(command);

        return response.IsSuccess
            ? CreatedAtAction(nameof(GetReview), new { shopId, reviewId = response.Data?.ReviewId }, response)
            : StatusCode(StatusCodes.Status409Conflict, response);
    }

    [HttpPut("{reviewId:guid}")]
    [ProducesResponseType(typeof(Response<UpdateCoffeeShopReviewResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateReview(
        [FromBody] UpdateCoffeeShopReviewRequest request, Guid reviewId)
    {
        request = request with { UserId = User.GetUserIdOrThrow(), ReviewId = reviewId };
        var response = await mediator.Send(request);

        return response.IsSuccess ? Ok(response) : NotFound(response);
    }

    [HttpDelete("{reviewId:guid}")]
    [ProducesResponseType(typeof(Response), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteReview(Guid shopId, Guid reviewId)
    {
        var command = new DeleteReviewFromCoffeeShopCommand(reviewId);
        var response = await mediator.Send(command);

        return response.IsSuccess ? NoContent() : NotFound(response);
    }

    private void AddPaginationHeaders(GetAllReviewsResponse data)
    {
        Response.Headers["X-Total-Count"] = data.TotalItems.ToString();
        Response.Headers["X-Total-Pages"] = data.TotalPages.ToString();
        Response.Headers["X-Current-Page"] = data.CurrentPage.ToString();
        Response.Headers["X-Page-Size"] = data.PageSize.ToString();
    }
}