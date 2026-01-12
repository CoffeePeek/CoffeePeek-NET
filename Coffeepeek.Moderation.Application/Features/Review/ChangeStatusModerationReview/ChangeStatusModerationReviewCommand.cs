using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Moderation.Application.Features.Review.ChangeStatusModerationReview;

public record ChangeStatusModerationReviewCommand(
    Guid UserId,
    Guid ModerationReviewId,
    ModerationStatus ModerationStatus,
    string? RejectReason) : IRequest<UpdateEntityResponse<ModerationStatus>>
{
    public Guid UserId { get; set; } = UserId;
}