using CoffeePeek.Account.Application.Features.DeleteUser;
using CoffeePeek.Account.Application.Features.GetProfile;
using CoffeePeek.Account.Application.Features.UpdateProfile;
using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Contract.Response.User;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.AccountService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IMediator mediator) : Controller
{
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<Response<UserDto>> GetProfile(CancellationToken cancellationToken)
    {
        var request = new GetProfileCommand(User.GetUserIdOrThrow());

        return mediator.Send(request, cancellationToken);
    }

    [HttpPut]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<Response<UpdateProfileResponse>> UpdateProfile([FromBody] UpdateProfileCommand command,
        CancellationToken cancellationToken)
    {
        var authenticatedRequest = command with { UserId = HttpContext.User.GetUserIdOrThrow() };
        return mediator.Send(authenticatedRequest, cancellationToken);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<Response<bool>> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        var request = new DeleteUserCommand(id);
        return mediator.Send(request, cancellationToken);
    }
}