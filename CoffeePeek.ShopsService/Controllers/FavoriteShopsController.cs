using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.Shared.Infrastructure;
using CoffeePeek.Shops.Application.Features.Favorite.AddToFavorite;
using CoffeePeek.Shops.Application.Features.Favorite.GetAllFavorites;
using CoffeePeek.Shops.Application.Features.Favorite.RemoveFromFavorite;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.ShopsService.Controllers;

[ApiController]
[Route("api/[controller]")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class FavoriteShopsController(IMediator mediator) : ControllerBase
{
    [HttpGet("all")]
    [Authorize]
    [ProducesResponseType(typeof(Response<GetAllFavoritesResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public Task<Response<GetAllFavoritesResponse>> GetAllFavorites()
    {
        return mediator.Send(new GetAllFavoritesCommand(User.GetUserIdOrThrow()));
    }
    
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CreateEntityResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public Task<CreateEntityResponse<Guid>> AddToFavorite([FromBody]Guid id)
    {
        return mediator.Send(new AddToFavoriteCommand(id, User.GetUserIdOrThrow()));
    }
    
    [HttpDelete()]
    [Authorize]
    [ProducesResponseType(typeof(UpdateEntityResponse<Guid>), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public Task<UpdateEntityResponse<Guid>> RemoveFromFavorite([FromQuery]Guid id)
    {
        var command = new RemoveFromFavoriteCommand(id, User.GetUserIdOrThrow());
        return mediator.Send(command);
    }
}