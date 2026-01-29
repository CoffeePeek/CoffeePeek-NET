using CoffeePeek.Contract.Dtos;
using CoffeePeek.Shared.Extensions.Exceptions;

namespace CoffeePeek.Moderation.Domain.Entities.ModerationReviewAggregate;

public partial class ModerationReview
{
    public static ModerationReview Create(Guid userId, Guid shopId, Guid moderationShopId, string userName, string header, string comment,
        RatingDto ratingDto, List<PhotoMetadata> photos)
    {
        if (shopId == Guid.Empty)
            throw new DomainException($"{nameof(shopId)} cannot be empty.");

        if (userId == Guid.Empty)
            throw new DomainException($"{nameof(userId)} cannot be empty.");
        
        if (moderationShopId == Guid.Empty)
            throw new DomainException($"{nameof(moderationShopId)} cannot be empty.");

        if (string.IsNullOrWhiteSpace(header))
            throw new DomainException("Review header is required.");

        if (header.Length is < BusinessConstants.MinReviewHeaderLength or > BusinessConstants.MaxReviewHeaderLength)
            throw new DomainException(
                $"{nameof(header)} must be between {BusinessConstants.MinReviewHeaderLength} and {BusinessConstants.MaxReviewHeaderLength} characters.");

        if (string.IsNullOrWhiteSpace(comment))
            throw new DomainException("Review comment is required.");

        if (comment.Length is < BusinessConstants.MinReviewCommentLength or > BusinessConstants.MaxReviewCommentLength)
            throw new DomainException(
                $"{nameof(comment)} must be between {BusinessConstants.MinReviewCommentLength} and {BusinessConstants.MaxReviewCommentLength} characters.");
        
        var rating = Rating.Create(ratingDto.Place, ratingDto.Service, ratingDto.Coffee);

        return new ModerationReview(userId, shopId, moderationShopId, userName, header, comment, rating, photos);
    }

    public void Approve(Guid moderatorId)
    {
        if (moderatorId == Guid.Empty)
            throw new DomainException($"{nameof(moderatorId)} cannot be empty.");

        if (ModerationStatus == Contract.Enums.ModerationStatus.Approved)
            throw new DomainException("Review is already approved.");

        ModeratedAt = DateTime.UtcNow;
        ModeratedBy = moderatorId;
        ModerationStatus = Contract.Enums.ModerationStatus.Approved;
    }
    
    public void Reject(string reason, Guid moderatorId)
    {
        if (moderatorId == Guid.Empty)
            throw new DomainException($"{nameof(moderatorId)} cannot be empty.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Reject reason is required.");

        if (reason.Length is < BusinessConstants.MinRejectReasonCommentLength or > BusinessConstants.MaxRejectReasonCommentLength)
            throw new DomainException(
                $"{nameof(reason)} must be between {BusinessConstants.MinRejectReasonCommentLength} and {BusinessConstants.MaxRejectReasonCommentLength} characters.");

        RejectedReason = reason;
        ModeratedAt = DateTime.UtcNow;
        ModeratedBy = moderatorId;
        ModerationStatus = Contract.Enums.ModerationStatus.Rejected;
    }
    
    public void MoveToPending(Guid moderatorId)
    {
        if (moderatorId == Guid.Empty)
            throw new DomainException($"{nameof(moderatorId)} cannot be empty.");

        RejectedReason = null;
        ModeratedAt = DateTime.UtcNow;
        ModeratedBy = moderatorId;
        ModerationStatus = Contract.Enums.ModerationStatus.Pending;
    }

    public void UpdateHeader(string header)
    {
        if (header == Header)
        {
            return;
        }
        
        if (header.Length is < BusinessConstants.MinReviewHeaderLength or > BusinessConstants.MaxReviewHeaderLength)
        {
            throw new DomainException(
                $"{nameof(header)} header must be between {BusinessConstants.MinReviewHeaderLength} and {BusinessConstants.MaxReviewHeaderLength} characters.");
        }
        
        Header = header;
    }

    public void UpdateComment(string comment)
    {
        if (comment.Length is < BusinessConstants.MinReviewCommentLength or > BusinessConstants.MaxReviewCommentLength)
        {
            throw new DomainException(
                $"{nameof(comment)} must be between {BusinessConstants.MinReviewCommentLength} and {BusinessConstants.MaxReviewCommentLength} characters.");
        }
        
        Comment = comment;
    }
}