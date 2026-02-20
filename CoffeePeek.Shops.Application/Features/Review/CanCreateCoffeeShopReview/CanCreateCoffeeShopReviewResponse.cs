namespace CoffeePeek.Shops.Application.Features.Review.CanCreateCoffeeShopReview;

/// <summary>
/// Represents the response indicating whether a coffee shop review can be created or edited.
/// </summary>
public record CanCreateCoffeeShopReviewResponse(bool CanCreate, Guid? ReviewId = null);