using System.Text.Json.Serialization;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Moderation.Application.Features.Review.ChangeStatusModerationReview;

public record ChangeStatusModerationReviewCommand(
    Guid ModerationReviewId,
    ModerationStatus ModerationStatus,
    string? RejectReason) : IRequest<UpdateEntityResponse<ModerationStatus>>
{
    [JsonIgnore] public Guid UserId { get; set; }
}