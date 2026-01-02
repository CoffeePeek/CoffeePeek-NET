using CoffeePeek.Contract.Requests.Internal;
using CoffeePeek.Contract.Response.Internal;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.Internal;
using CoffeePeek.Shops.Application.Commands.Internal;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.ShopsService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InternalController(IMediator mediator) : Controller
{
    [HttpGet("cities")]
    public Task<Response<GetCitiesResponse>> GetCities()
    {
        var request = new GetCitiesCommand();
        
        return mediator.Send(request);
    }

    [HttpGet("beans")]
    public Task<Response<GetAllBeansResponse>> GetAllBeans()
    {
        var command = new GetAllBeansCommand();
        return mediator.Send(command);
    }
    
    [HttpGet("equipments")]
    public Task<Response<GetAllEquipmentResponse>> GetAllEquipment()
    {
        var command = new GetAllEquipmentCommand();
        return mediator.Send(command);
    }
    
    [HttpGet("roasters")]
    public Task<Response<GetAllRoastersResponse>> GetAllRoasters()
    {
        var command = new GetAllRoastersCommand();
        return mediator.Send(command);
    }
    
    [HttpGet("brew-methods")]
    public Task<Response<GetAllBrewMethodsResponse>> GetBrewMethods()
    {
        var command = new GetAllBrewMethodsCommand();
        return mediator.Send(command);
    }
    
    [HttpGet("statistics/{userId:guid}")]
    public Task<Response<GetUserStatisticsResponse>> GetUserStatistics(Guid userId)
    {
        var command = new GetUserStatisticsCommand(userId);
        return mediator.Send(command);
    }
}