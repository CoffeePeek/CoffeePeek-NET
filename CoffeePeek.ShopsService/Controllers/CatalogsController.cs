using CoffeePeek.Contract.Abstract;
using CoffeePeek.Shops.Application.Features.Internal.GetAllBeans;
using CoffeePeek.Shops.Application.Features.Internal.GetAllBrewMethods;
using CoffeePeek.Shops.Application.Features.Internal.GetAllCities;
using CoffeePeek.Shops.Application.Features.Internal.GetAllEquipment;
using CoffeePeek.Shops.Application.Features.Internal.GetAllRoasters;
using CoffeePeek.Shops.Application.Features.Internal.GetUserStatistics;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.ShopsService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogsController(IMediator mediator) : Controller
{
    [HttpGet("cities")]
    [ProducesResponseType(typeof(Response<GetCitiesResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCities()
    {
        var request = new GetCitiesCommand();
        var response = await mediator.Send(request);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpGet("beans")]
    [ProducesResponseType(typeof(Response<GetAllBeansResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBeans()
    {
        var command = new GetAllBeansCommand();
        var response = await mediator.Send(command);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpGet("equipments")]
    [ProducesResponseType(typeof(Response<GetAllEquipmentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEquipment()
    {
        var command = new GetAllEquipmentCommand();
        var response = await mediator.Send(command);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpGet("roasters")]
    [ProducesResponseType(typeof(Response<GetAllRoastersResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoasters()
    {
        var command = new GetAllRoastersCommand();
        var response = await mediator.Send(command);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpGet("brew-methods")]
    [ProducesResponseType(typeof(Response<GetAllBrewMethodsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBrewMethods()
    {
        var command = new GetAllBrewMethodsCommand();
        var response = await mediator.Send(command);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }
}