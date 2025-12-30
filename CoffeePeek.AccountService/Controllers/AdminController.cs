using CoffeePeek.Account.Application.Features.Admin.ChangeRole;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Infrastructure;
using CoffeePeek.Shared.Infrastructure.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.AccountService.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
public class AdminController(IMediator mediator) : ControllerBase
{
    [HttpPut("role")]
    [Authorize(Policy = RoleConsts.Admin)]
    public Task<Response> ChangeRole([FromQuery] Guid userIdOfChange, [FromQuery]Guid roleId)
    {
        return mediator.Send(new ChangeRoleCommand(User.GetUserIdOrThrow(), userIdOfChange, roleId));
    }
}