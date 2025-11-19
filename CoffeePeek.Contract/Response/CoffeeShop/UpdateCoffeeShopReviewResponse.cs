namespace CoffeePeek.Contract.Response.CoffeeShop;

public class UpdateCoffeeShopReviewResponse(int reviewId)
{
    public int ReviewId { get; } = reviewId;
}