namespace CoffeePeek.Contract.Response.CoffeeShop;

public class AddCoffeeShopReviewResponse(int reviewId)
{
    public int ReviewId { get; } = reviewId;
}