using CoffeePeek.Account.Application.Features.Auth.CheckUserExistsByEmail;
using CoffeePeek.Account.Application.Features.Auth.Email.ConfirmEmail;
using CoffeePeek.Account.Application.Features.Auth.Email.ResendEmailConfirmation;
using CoffeePeek.Account.Application.Features.Auth.RegisterUser;
using CoffeePeek.Account.Application.Features.DeleteUser;
using CoffeePeek.Account.Application.Features.User.GetProfile;
using CoffeePeek.Account.Application.Features.User.UpdateEmail;
using CoffeePeek.Account.Application.Features.User.UpdateProfile;
using CoffeePeek.Account.Application.Features.User.UpdateUserAvatar;
using CoffeePeek.Account.Domain.Entities;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Dtos;
using CoffeePeek.Shared.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CoffeePeek.AccountService.Controllers;

[ApiController]
[Route("api/[controller]")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class UsersController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType<Response<UserProfileResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType( StatusCodes.Status404NotFound)]
    [ProducesResponseType( StatusCodes.Status500InternalServerError)]
    [SwaggerOperation("Get user profile by id")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await mediator.Send(new GetPublicUserProfileQuery(id));
        return Ok(result);
    }
    
    [HttpGet("exists")] 
    [ProducesResponseType<Response>(StatusCodes.Status200OK)]
    [ProducesResponseType( StatusCodes.Status404NotFound)]
    [ProducesResponseType( StatusCodes.Status500InternalServerError)]
    [SwaggerOperation("Check if user exists by email")]
    public async Task<IActionResult> CheckUser([FromQuery] string email)
    {
        var request = new CheckUserExistsByEmailQuery(email);
        
        var response = await mediator.Send(request);
        
        return response.Data ? Ok(response) : NotFound(response);
    }

    [HttpPost]
    [ProducesResponseType<CreateEntityResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation("Register new user")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var response = await mediator.Send(command);
        return Ok(response);
    }

    #region Me
    
    [HttpPatch("me")]
    [Authorize]
    [ProducesResponseType<Response<UpdateProfileResponse>>( StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation("Update user profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var command = request with { UserId = HttpContext.User.GetUserIdOrThrow() };
        var response = await mediator.Send(command, cancellationToken);
        
        return Ok(response);
    }

    [HttpPut("me/avatar")]
    [Authorize]
    [ProducesResponseType<UpdateEntityResponse<PhotoMetadata>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation("Update user avatar")]
    public async Task<IActionResult> UpdateAvatar([FromBody] UploadedPhotoDto dto)
    {
        var command = new UpdateUserAvatarCommand(User.GetUserIdOrThrow(), dto);
        var result = await mediator.Send(command);
        
        return Ok(result);
    }

    [HttpPatch("me/email")]
    [Authorize]
    [ProducesResponseType<UpdateEntityResponse<string>>(StatusCodes.Status202Accepted)]
    [ProducesResponseType( StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation("Update user email")]
    public async Task<IActionResult> UpdateEmail([FromQuery] string email, CancellationToken cancellationToken)
    {
        var command = new UpdateEmailCommand(HttpContext.User.GetUserIdOrThrow(), email);
        var result = await mediator.Send(command, cancellationToken);

        return Accepted(result);
    }
    
    [HttpPost("me/email-confirmation")]
    [Authorize]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation("Resend email confirmation")]
    public Task<Response> ResendEmailConfirm()
    {
        return mediator.Send(new ResendEmailConfirmationCommand(User.GetUserIdOrThrow()));
    }
    
    [HttpPut("me/email-confirmation")]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation("Confirm email")]
    public Task<Response> ConfirmEmail([FromQuery] string token)
    {
        return mediator.Send(new ConfirmEmailCommand(token));
    }
    
    [HttpDelete("me/{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation("Delete user by id")]
    public Task<Response<bool>> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        var request = new DeleteUserCommand(id);
        return mediator.Send(request, cancellationToken);
    }
    
    #endregion
}