using CoffeePeek.Api.Extensions;
using CoffeePeek.Contract.Constants;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoffeeShopController(IMediator mediator) : Controller
{
    [HttpGet]
    [ProducesResponseType(typeof(Response<GetCoffeeShopsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<Response<GetCoffeeShopsResponse>> GetCoffeeShops(
        [FromQuery] int cityId,
        [FromHeader(Name = "X-Page-Number")] int pageNumber = 1,
        [FromHeader(Name = "X-Page-Size")] int pageSize = 10)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            cityId = BusinessConstants.DefaultUnAuthorizedCityId;
        }

        var response = await mediator.Send(new GetCoffeeShopsRequest(cityId, pageNumber, pageSize));

        AddPaginationHeaders(response.Data);

        return response;
        
        void AddPaginationHeaders(GetCoffeeShopsResponse data)
        {
            Response.Headers.TryAdd("X-Total-Count", data.TotalItems.ToString());
            Response.Headers.TryAdd("X-Total-Pages", data.TotalPages.ToString());
            Response.Headers.TryAdd("X-Current-Page", data.CurrentPage.ToString());
            Response.Headers.TryAdd("X-Page-Size", data.PageSize.ToString());
        }
    }

    [HttpPost("review")]
    [Authorize]
    public Task<Response<AddCoffeeShopReviewResponse>> AddCoffeeShopReview([FromBody]AddCoffeeShopReviewRequest request)
    {
        request.UserId = HttpContext.GetUserIdOrThrow();
        return mediator.Send(request);
    }
    
    [HttpPut("review")]
    [Authorize]
    public Task<Response<UpdateCoffeeShopReviewResponse>> UpdateCoffeeShopReview([FromBody]UpdateCoffeeShopReviewRequest request)
    {
        request.UserId = HttpContext.GetUserIdOrThrow();
        return mediator.Send(request);
    }
    
    
    //[HttpPost("send-to-review")]
    //[Authorize]
    //public Task<Response<UpdateCoffeeShopResponse>> UpdateCoffeeShop([FromForm] UpdateCoffeeShopRequest request)
    //{
    //    return mediator.Send(request);
    //}
}