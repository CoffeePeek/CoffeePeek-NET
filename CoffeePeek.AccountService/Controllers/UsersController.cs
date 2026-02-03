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
    /// <summary>
    /// Получает публичный профиль пользователя по заданному идентификатору; если параметр `id` равен null, возвращает профиль текущего пользователя.
    /// </summary>
    /// <param name="id">Идентификатор пользователя. Если не указан, используется идентификатор текущего пользователя из контекста.</param>
    /// <returns>Response с данными профиля пользователя (UserProfileResponse).</returns>
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

    /// <summary>
    /// Возвращает публичный профиль текущего пользователя.
    /// </summary>
    /// <returns>HTTP-ответ, содержащий объект Response&lt;UserProfileResponse&gt; с данными профиля или соответствующий код ошибки (например 404 или 500).</returns>
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

    /// <summary>
    /// Обновляет поле «about» в профиле текущего аутентифицированного пользователя.
    /// </summary>
    /// <param name="request">Команда с новым значением поля «about». Поле `UserId` будет заменено на идентификатор текущего пользователя из контекста.</param>
    /// <returns>UpdateEntityResponse<string> с обновлённым значением поля «about».</returns>
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

    /// <summary>
    /// Обновляет адрес электронной почты профиля текущего пользователя.
    /// </summary>
    /// <param name="request">Команда с новым адресом электронной почты; идентификатор пользователя устанавливается автоматически из контекста.</param>
    /// <returns>UpdateEntityResponse<string> с обновлённым адресом электронной почты или информацией об результате операции.</returns>
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

    /// <summary>
    /// Обновляет номер телефона текущего пользователя.
    /// </summary>
    /// <param name="request">Команда с новым номером телефона; поле `UserId` будет заменено на идентификатор текущего пользователя.</param>
    /// <returns>Accepted (202) с `UpdateEntityResponse<string>`, содержащим обновлённый номер телефона.</returns>
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

    /// <summary>
    /// Обновляет аватар текущего пользователя.
    /// </summary>
    /// <param name="dto">Данные загружаемого изображения (пиксельные/файловые метаданные и содержимое) для установки в качестве аватара.</param>
    /// <returns>Ответ с HTTP 202 и телом типа <see cref="UpdateEntityResponse{PhotoMetadata}"/> при принятии запроса; может возвращать 400, 404 или 500 в ошибочных случаях.</returns>
    [HttpPut("me/avatar")]
    [Authorize]
    [ProducesResponseType<UpdateEntityResponse<PhotoMetadata>>(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation("Update user avatar")]
    public async Task<IActionResult> UpdateAvatar([FromBody] UploadedPhotoDto dto)
    {
        var command = new UpdateUserAvatarCommand(userContext.GetUserIdOrThrow(), dto);
        var response = await mediator.Send(command);

        return Accepted(response);
    }

    /// <summary>
    /// Обновляет имя пользователя для текущего аутентифицированного пользователя.
    /// </summary>
    /// <param name="command">Команда с новым именем пользователя; поле UserId устанавливается в идентификатор текущего пользователя.</param>
    /// <returns>UpdateEntityResponse&lt;string&gt; с обновлённым именем пользователя.</returns>
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

    /// <summary>
    /// Удаляет текущего аутентифицированного пользователя по идентификатору из контекста пользователя.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены для прерывания операции.</param>
    /// <returns>Response, содержащий `true`, если пользователь успешно удалён, иначе `false`.</returns>
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


    /// <summary>
    /// Повторно отправляет подтверждение электронной почты текущему пользователю.
    /// </summary>
    /// <returns>`Response` с результатом операции: `true`, если письмо было инициировано для отправки, `false` в противном случае.</returns>
    [HttpPost("me/email-confirmation")]
    [Authorize]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation("Resend email confirmation")]
    public Task<Response> ResendEmailConfirm()
    {
        return mediator.Send(new ResendEmailConfirmationCommand(userContext.GetUserIdOrThrow()));
    }

    /// <summary>
    /// Подтверждает адрес электронной почты с помощью переданного токена подтверждения.
    /// </summary>
    /// <param name="token">Токен подтверждения, переданный в строке запроса.</param>
    /// <returns>Объект Response с результатом операции подтверждения электронной почты.</returns>
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