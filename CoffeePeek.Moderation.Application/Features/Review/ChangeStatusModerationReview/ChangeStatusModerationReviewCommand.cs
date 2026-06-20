using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Moderation.Domain;

namespace CoffeePeek.Moderation.Application.Features.Review.ChangeStatusModerationReview;

public record ChangeStatusModerationReviewCommand(
    [property: JsonIgnore] Guid UserId,
    Guid ModerationReviewId,
    ModerationStatus ModerationStatus,
    [property: MaxLength(BusinessConstants.MaxRejectReasonCommentLength)] string? Comment,
    [property: MaxLength(BusinessConstants.MaxRejectReasonCommentLength)] string? RejectReason);
