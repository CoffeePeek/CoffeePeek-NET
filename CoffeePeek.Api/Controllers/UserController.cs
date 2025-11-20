using CoffeePeek.Api.Extensions;
using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Contract.Requests.User;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.User;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IMediator mediator, IHub hub) : Controller
{
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<Response<UserDto>> GetProfile(CancellationToken cancellationToken)
    {
        var request = new GetProfileRequest(HttpContext.GetUserIdOrThrow());
        return mediator.Send(request, cancellationToken);
    }
    
    [HttpPut]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<Response<UpdateProfileResponse>> UpdateProfile([FromBody]UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var authenticatedRequest = request with { UserId = HttpContext.GetUserIdOrThrow() };
        return mediator.Send(authenticatedRequest, cancellationToken);
    }
    
    [HttpGet("Users")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<Response<UserDto[]>> GetAllUsers(CancellationToken cancellationToken)
    {
        hub.GetSpan()?.StartChild("additional-work");
        
        var request = new GetAllUsersRequest();
        var result = await mediator.Send(request, cancellationToken);
        
        return result;
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<Response<bool>> DeleteUser(int id, CancellationToken cancellationToken)
    {
        var request = new DeleteUserRequest(id);
        return mediator.Send(request, cancellationToken);
    }
}