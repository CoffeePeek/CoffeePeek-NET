using System.ComponentModel;
using CoffeePeek.Account.Application.Features.DeleteUser;
using CoffeePeek.Account.Application.Features.GetProfile;
using CoffeePeek.Account.Application.Features.User.Email.ConfirmEmail;
using CoffeePeek.Account.Application.Features.User.Email.ResendEmailConfirmation;
using CoffeePeek.Account.Application.Features.User.UpdateEmail;
using CoffeePeek.Account.Application.Features.User.UpdateProfile;
using CoffeePeek.Account.Application.Features.User.UpdateUserAvatar;
using CoffeePeek.Account.Application.Features.User.UpdateUserAvatar.GenerateUploadAvatarUrl;
using CoffeePeek.Account.Domain.Aggregates;
using CoffeePeek.Contract.Dtos;
using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Contract.Requests;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.Contract.Responses.User;
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

    [HttpPost("upload-url")]
    [Authorize]
    [ProducesResponseType(typeof(Response<GenerateUploadUrlResponse>), StatusCodes.Status200OK)]
    [Description("Get url for presigned upload avatar photo")]
    public async Task<Response<GenerateUploadUrlResponse>> GenerateUploadUrl([FromBody] UploadUrlRequest request)
    {
        var command = new GenerateUploadAvatarUrlCommand(request);
        return await mediator.Send(command);
    }

    [HttpPut("avatar")]
    [Authorize]
    [ProducesResponseType(typeof(PhotoMetadata), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<UpdateEntityResponse<PhotoMetadata>> UpdateAvatar([FromBody] UploadedPhotoDto dto)
    {
        var command = new UpdateUserAvatarCommand(User.GetUserIdOrThrow(), dto);
        return await mediator.Send(command);
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

    [HttpPut("email")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<UpdateEntityResponse<string>> UpdateEmail([FromBody] UpdateEmailCommand command,
        CancellationToken cancellationToken)
    {
        var updateEmailCommand = command with { UserId = HttpContext.User.GetUserIdOrThrow() };
        return mediator.Send(updateEmailCommand, cancellationToken);
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

    [HttpPost("resend-email-confirm")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public Task<Response> ResendEmailConfirm()
    {
        return mediator.Send(new ResendEmailConfirmationCommand(User.GetUserIdOrThrow()));
    }
    
    [HttpPost("confirm-email")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public Task<Response> ConfirmEmail([FromQuery] string token)
    {
        return mediator.Send(new ConfirmEmailCommand(token));
    }
}