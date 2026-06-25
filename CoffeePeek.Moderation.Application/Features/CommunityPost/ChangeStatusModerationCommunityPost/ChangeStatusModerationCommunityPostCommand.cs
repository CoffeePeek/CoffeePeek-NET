using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Moderation.Domain;

namespace CoffeePeek.Moderation.Application.Features.CommunityPost.ChangeStatusModerationCommunityPost;

public record ChangeStatusModerationCommunityPostCommand(
    [property: JsonIgnore] Guid UserId,
    Guid ModerationCommunityPostId,
    ModerationStatus ModerationStatus,
    [property: MaxLength(BusinessConstants.MaxRejectReasonCommentLength)] string? Comment,
    [property: MaxLength(BusinessConstants.MaxRejectReasonCommentLength)] string? RejectReason);
