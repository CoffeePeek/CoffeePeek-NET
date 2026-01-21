using System.Text.Json.Serialization;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Moderation.Application.Features.Review.ChangeStatusModerationReview;

public record ChangeStatusModerationReviewCommand(
    [property: JsonIgnore] Guid UserId,
    Guid ModerationReviewId,
    ModerationStatus ModerationStatus,
    string? RejectReason) : IRequest<UpdateEntityResponse<ModerationStatus>>;