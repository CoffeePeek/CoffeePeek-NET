using CoffeePeek.Moderation.Domain;
using CoffeePeek.Moderation.Domain.Common.Enums;
using CoffeePeek.Shared.Kernel.Exceptions;

namespace CoffeePeek.Moderation.Domain.Aggregates.ModerationCommunityPostAggregate;

public sealed partial class ModerationCommunityPost
{
    public static ModerationCommunityPost Create(
        Guid userId,
        string userName,
        ModerationCommunityPostType postType,
        string title,
        string body,
        Guid? linkedShopId)
    {
        if (userId == Guid.Empty)
            throw new DomainException($"{nameof(userId)} cannot be empty.");

        if (string.IsNullOrWhiteSpace(userName))
            throw new DomainException("UserName is required.");

        if (!Enum.IsDefined(postType))
            throw new DomainException("Invalid post type.");

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Post title is required.");

        var trimmedTitle = title.Trim();
        if (trimmedTitle.Length is < BusinessConstants.MinCommunityPostTitleLength
            or > BusinessConstants.MaxCommunityPostTitleLength)
        {
            throw new DomainException(
                $"Title must be between {BusinessConstants.MinCommunityPostTitleLength} and {BusinessConstants.MaxCommunityPostTitleLength} characters.");
        }

        if (string.IsNullOrWhiteSpace(body))
            throw new DomainException("Post body is required.");

        var trimmedBody = body.Trim();
        if (trimmedBody.Length is < BusinessConstants.MinCommunityPostBodyLength
            or > BusinessConstants.MaxCommunityPostBodyLength)
        {
            throw new DomainException(
                $"Body must be between {BusinessConstants.MinCommunityPostBodyLength} and {BusinessConstants.MaxCommunityPostBodyLength} characters.");
        }

        if (linkedShopId is { } shopId && shopId == Guid.Empty)
            throw new DomainException($"{nameof(linkedShopId)} cannot be empty.");

        return new ModerationCommunityPost(userId, userName.Trim(), postType, trimmedTitle, trimmedBody, linkedShopId);
    }

    public void Approve(Guid moderatorId)
    {
        if (moderatorId == Guid.Empty)
            throw new DomainException($"{nameof(moderatorId)} cannot be empty.");

        if (ModerationStatus == ModerationStatus.Approved)
            throw new DomainException("Post is already approved.");

        ModeratedAt = DateTime.UtcNow;
        ModeratedBy = moderatorId;
        ModerationStatus = ModerationStatus.Approved;
    }

    public void Reject(string reason, Guid moderatorId)
    {
        if (moderatorId == Guid.Empty)
            throw new DomainException($"{nameof(moderatorId)} cannot be empty.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Reject reason is required.");

        if (reason.Length is < BusinessConstants.MinRejectReasonCommentLength
            or > BusinessConstants.MaxRejectReasonCommentLength)
        {
            throw new DomainException(
                $"Reject reason must be between {BusinessConstants.MinRejectReasonCommentLength} and {BusinessConstants.MaxRejectReasonCommentLength} characters.");
        }

        RejectedReason = reason.Trim();
        ModeratedAt = DateTime.UtcNow;
        ModeratedBy = moderatorId;
        ModerationStatus = ModerationStatus.Rejected;
    }

    public void MoveToPending(Guid moderatorId)
    {
        if (moderatorId == Guid.Empty)
            throw new DomainException($"{nameof(moderatorId)} cannot be empty.");

        RejectedReason = null;
        ModeratedAt = DateTime.UtcNow;
        ModeratedBy = moderatorId;
        ModerationStatus = ModerationStatus.Pending;
    }
}
