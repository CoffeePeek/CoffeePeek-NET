namespace CoffeePeek.Moderation.Domain.Entities.ModerationReviewAggregate;

public partial class ModerationReview
{
    public static ModerationReview Create(Guid userId, Guid shopId, string header, string comment, int ratingPlace,
        int ratingService, int ratingCoffee)
    {
        return new ModerationReview(userId, shopId, header, comment, ratingCoffee, ratingPlace, ratingService);
    }

    public void Approve(Guid moderatorId)
    {
        ModeratedAt = DateTime.UtcNow;
        ModeratedBy = moderatorId;
        ModerationStatus = Contract.Enums.ModerationStatus.Approved;
        
        AddDomainEvent(new ModerationReviewApprovedDomainEvent(this));
    }
    
    public void Reject(string reason, Guid moderatorId)
    {
        RejectedReason = reason;
        ModeratedAt = DateTime.UtcNow;
        ModeratedBy = moderatorId;
        ModerationStatus = Contract.Enums.ModerationStatus.Rejected;
    }
}