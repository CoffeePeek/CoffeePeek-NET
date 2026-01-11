namespace CoffeePeek.Contract.Response.CoffeeShop;

public class CreateCoffeeShopReviewResponse(Guid reviewId)
{
    public Guid ReviewId { get; } = reviewId;
}