using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Moderation.Application.Features.Review.ChangeStatusModerationReview;
using CoffeePeek.Moderation.Application.Features.Review.GetAllModerationReviews;
using CoffeePeek.Moderation.Application.Features.Review.SendReviewToModeration;
using CoffeePeek.Moderation.Application.Features.Review.UpdateCoffeeShopReview;
using CoffeePeek.Shared.Infrastructure;
using CoffeePeek.Shared.Infrastructure.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CoffeePeek.ModerationService.Controllers;

[ApiController]
[Route("api/[controller]")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class ModerationReviewsController(IMediator mediator, IUserContext userContext) : ControllerBase
{
    /// <summary>
    /// Получает все модерационные отзывы.
    /// </summary>
    /// <returns>HTTP 200 с объектом Response&lt;GetAllModerationReviewsResponse&gt;, содержащим список модерационных отзывов.</returns>
    [HttpGet]
    [Authorize(Policy = RoleConsts.Moderator)]
    [ProducesResponseType(typeof(Response<GetAllModerationReviewsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [SwaggerOperation("Get all moderation reviews")]
    public async Task<IActionResult> GetAllModerationReviews()
    {
        return Ok(await mediator.Send(new GetAllModerationReviewsQuery()));
    }

    /// <summary>
    /// Отправляет созданный пользователем запрос на модерацию отзыва и создаёт запись модерации от имени текущего пользователя.
    /// </summary>
    /// <param name="command">Команда с данными отзыва для отправки на модерацию (тело запроса).</param>
    /// <returns>`CreateEntityResponse` с идентификатором созданной сущности модерации.</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CreateEntityResponse), statusCode: StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [SwaggerOperation("Create new moderation review")]
    public async Task<IActionResult> SendReviewToModeration([FromBody] SendReviewToModerationCommand command)
    {
        command = command with
        {
            UserId = userContext.GetUserIdOrThrow(),
            UserName = userContext.GetUsernameOrThrow()
        };

        var response = await mediator.Send(command);

        return Ok(response);
    }

    /// <summary>
    /// Обновляет модерационный отзыв с указанным идентификатором от имени текущего пользователя.
    /// </summary>
    /// <param name="command">Данные обновления отзыва.</param>
    /// <param name="reviewId">Идентификатор отзыва, который требуется обновить.</param>
    /// <returns>HTTP 200 с Response&lt;UpdateCoffeeShopReviewResponse&gt; при успешном обновлении; HTTP 404 с ответом, если отзыв не найден.</returns>
    [HttpPut("{reviewId:guid}")]
    [ProducesResponseType(typeof(Response<UpdateCoffeeShopReviewResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [SwaggerOperation("Update moderation review")]
    public async Task<IActionResult> UpdateReview(
        [FromBody] UpdateCoffeeShopReviewCommand command, Guid reviewId)
    {
        command = command with { UserId = userContext.GetUserIdOrThrow(), ReviewId = reviewId };
        var response = await mediator.Send(command);

        return response.IsSuccess ? Ok(response) : NotFound(response);
    }

    /// <summary>
    /// Изменяет статус модируемого отзыва; при установке статуса отвергнутого требуется указать причину отклонения.
    /// </summary>
    /// <param name="command">Команда с идентификатором отзыва и новым значением статуса; поле UserId заполняется текущим пользователем из контекста.</param>
    /// <returns>UpdateEntityResponse&lt;ModerationStatus&gt; с результатом операции и итоговым статусом модерации.</returns>
    [HttpPut]
    [Authorize(Policy = RoleConsts.Moderator)]
    [ProducesResponseType<UpdateEntityResponse<ModerationStatus>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [SwaggerOperation("Change status of moderation review, when reject status need set reject reason")]
    public async Task<IActionResult> ChangeStatusModerationReview(
        ChangeStatusModerationReviewCommand command)
    {
        var commandWithUser = command with { UserId = userContext.GetUserIdOrThrow() };
        return Ok(await mediator.Send(commandWithUser));
    }
}