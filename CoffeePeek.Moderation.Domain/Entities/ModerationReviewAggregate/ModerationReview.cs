using CoffeePeek.Contract.Enums;
using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Moderation.Domain.Entities.ModerationReviewAggregate;

public partial class ModerationReview : Entity<Guid>
{
    public string Header { get; private set; }
    public string Comment { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ShopId { get; private set; }

    public int RatingCoffee { get; private set; }
    public int RatingPlace { get; private set; }
    public int RatingService { get; private set; }

    public string? RejectedReason  { get; private set; }
    public Guid? ModeratedBy  { get; private set; }
    public DateTime ModeratedAt  { get; private set; }
    public ModerationStatus ModerationStatus { get; private set; }
    
    public bool IsSoftDelete { get; private set; }

    // ReSharper disable once UnusedMember.Local
    private ModerationReview()
    {
        
    }

    public ModerationReview(Guid userId, Guid shopId, string header, string comment, int ratingCoffee, int ratingPlace,
        int ratingService)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        ShopId = shopId;
        Header = header;
        Comment = comment;
        RatingCoffee = ratingCoffee;
        RatingPlace = ratingPlace;
        RatingService = ratingService;
        ModerationStatus = ModerationStatus.Pending;
    }
}