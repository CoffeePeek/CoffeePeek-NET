using CoffeePeek.Contract.Enums;

namespace CoffeePeek.Contract.Dtos.CoffeeShop;

public class ModerationReviewDto
{
    public string Header { get; init; }
    public string Comment { get;init; }
    public Guid UserId { get; init; }
    public Guid ShopId { get;init; }

    public int RatingCoffee { get; init; }
    public int RatingPlace { get; init; }
    public int RatingService { get; init; }

    public string? RejectedReason  { get; init; }
    public Guid? ModeratedBy  { get; init; }
    public DateTime ModeratedAt  { get; init; }
    public ModerationStatus ModerationStatus { get; init; }
}