using CoffeePeek.Account.Application.Features.Auth.CheckUserExistsByEmail;
using CoffeePeek.Account.Application.Features.Auth.Email.ConfirmEmail;
using CoffeePeek.Account.Application.Features.Auth.Email.ResendEmailConfirmation;
using CoffeePeek.Account.Application.Features.Auth.RegisterUser;
using CoffeePeek.Account.Application.Features.User.DeleteUser;
using CoffeePeek.Account.Application.Features.User.GetProfile;
using CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdateAbout;
using CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdateAvatar;
using CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdateEmail;
using CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdatePhoneNumber;
using CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdateUsername;
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
public class UsersController(IMediator mediator, IUserContext userContext) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType<Response<UserProfileResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation("Get user profile by id or current user profile")]
    public async Task<IActionResult> GetById(Guid? id)
    {
        var response = await mediator.Send(new GetPublicUserProfileQuery(id ?? userContext.GetUserIdOrThrow()));
        return Ok(response);
    }

    [HttpGet("exists")]
    [ProducesResponseType<Response>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation("Check if user exists by email")]
    public async Task<IActionResult> CheckUser([FromQuery] string email)
    {
        var request = new CheckUserExistsByEmailQuery(email);

        var response = await mediator.Send(request);

        return Ok(response);
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

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType<Response<UserProfileResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation("Get user profile by id or current user profile")]
    public async Task<IActionResult> GetById()
    {
        var response = await mediator.Send(new GetPublicUserProfileQuery(userContext.GetUserIdOrThrow()));
        return Ok(response);
    }

    [HttpPatch("me/about")]
    [Authorize]
    [ProducesResponseType<UpdateEntityResponse<string>>(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation("Update user profile about information")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileAboutCommand request,
        CancellationToken cancellationToken)
    {
        var command = request with { UserId = userContext.GetUserIdOrThrow() };
        var response = await mediator.Send(command, cancellationToken);

        return Accepted(response);
    }

    [HttpPatch("me/email")]
    [Authorize]
    [ProducesResponseType<UpdateEntityResponse<string>>(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation("Update user profile email")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileEmailCommand request,
        CancellationToken cancellationToken)
    {
        var command = request with { UserId = userContext.GetUserIdOrThrow() };
        var response = await mediator.Send(command, cancellationToken);

        return Accepted(response);
    }

    [HttpPatch("me/phone-number")]
    [Authorize]
    [ProducesResponseType<UpdateEntityResponse<string>>(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation("Update user profile phone number")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfilePhoneNumberCommand request,
        CancellationToken cancellationToken)
    {
        var command = request with { UserId = userContext.GetUserIdOrThrow() };
        var response = await mediator.Send(command, cancellationToken);

        return Accepted(response);
    }

    [HttpPatch("me/avatar")]
    [Authorize]
    [ProducesResponseType<UpdateEntityResponse<PhotoMetadata>>(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation("Update user avatar")]
    public async Task<IActionResult> UpdateAvatar([FromBody] UpdateUserAvatarCommand command)
    {
        command = command with { UserId = userContext.GetUserIdOrThrow() };
        var response = await mediator.Send(command);

        return Accepted(response);
    }

    [HttpPatch("me/username")]
    [Authorize]
    [ProducesResponseType<UpdateEntityResponse<string>>(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation("Update user username")]
    public async Task<IActionResult> UpdateEmail([FromBody] UpdateProfileUsernameCommand command,
        CancellationToken cancellationToken)
    {
        command = command with
        {
            UserId = userContext.GetUserIdOrThrow()
        };

        var response = await mediator.Send(command, cancellationToken);

        return Accepted(response);
    }

    [HttpDelete("me")]
    [Authorize]
    [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation("Delete user by id")]
    public Task<Response<bool>> DeleteUser(CancellationToken cancellationToken)
    {
        var request = new DeleteUserCommand(userContext.GetUserIdOrThrow());
        return mediator.Send(request, cancellationToken);
    }


    [HttpPost("me/email-confirmation")]
    [Authorize]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation("Resend email confirmation")]
    public Task<Response> ResendEmailConfirm()
    {
        return mediator.Send(new ResendEmailConfirmationCommand(userContext.GetUserIdOrThrow()));
    }

    [HttpPut("me/email-confirmation")]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation("Confirm email")]
    public Task<Response> ConfirmEmail([FromQuery] string token)
    {
        return mediator.Send(new ConfirmEmailCommand(token));
    }

    #endregion
}