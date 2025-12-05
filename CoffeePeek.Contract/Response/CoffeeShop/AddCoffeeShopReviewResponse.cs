namespace CoffeePeek.Contract.Response.CoffeeShop;

public class AddCoffeeShopReviewResponse(Guid reviewId)
{
    public Guid ReviewId { get; } = reviewId;
}