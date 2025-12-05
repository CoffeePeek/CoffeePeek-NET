using CoffeePeek.Contract.Constants;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.ShopsService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoffeeShopController(IMediator mediator) : Controller
{
    [HttpGet]
    [ProducesResponseType(typeof(Response<GetCoffeeShopsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<Response<GetCoffeeShopsResponse>> GetCoffeeShops(
        [FromQuery] Guid? cityId,
        [FromHeader(Name = "X-Page-Number")] int pageNumber = 1,
        [FromHeader(Name = "X-Page-Size")] int pageSize = 10)
    {
        cityId ??= BusinessConstants.DefaultUnAuthorizedCityId;
        
        var response = await mediator.Send(new GetCoffeeShopsCommand(cityId.Value, pageNumber, pageSize));

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

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Response<GetCoffeeShopResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public Task<Response<GetCoffeeShopResponse>> GetCoffeeShop(Guid id)
    {
        return mediator.Send(new GetCoffeeShopCommand(id));
    }
    
    [HttpPost("/favorite{id:guid}")]
    [ProducesResponseType(typeof(Response<GetCoffeeShopResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public Task<CreateEntityResponse<Guid>> AddToFavorite([FromBody]Guid id, Guid userId)
    {
        return mediator.Send(new AddToFavoriteCommand(id, userId));
    }
    
    [HttpDelete("/favorite{id:guid}")]
    [ProducesResponseType(typeof(Response<GetCoffeeShopResponse>), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public Task<UpdateEntityResponse<Guid>> RemoveFromFavorite([FromBody]Guid id, Guid userId)
    {
        return mediator.Send(new RemoveFromFavoriteCommand(id, userId));
    }
}