using System.Text.Json.Serialization;
using CoffeePeek.Contract.Enums;

namespace CoffeePeek.Moderation.Application.Features.Review.ChangeStatusModerationReview;

public record ChangeStatusModerationReviewCommand(
    [property: JsonIgnore] Guid UserId,
    Guid ModerationReviewId,
    ModerationStatus ModerationStatus,
    string? RejectReason);