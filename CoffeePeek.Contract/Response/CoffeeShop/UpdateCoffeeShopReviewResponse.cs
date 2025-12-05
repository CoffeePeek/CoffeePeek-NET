namespace CoffeePeek.Contract.Response.CoffeeShop;

public class UpdateCoffeeShopReviewResponse(Guid reviewId)
{
    public Guid ReviewId { get; } = reviewId;
}