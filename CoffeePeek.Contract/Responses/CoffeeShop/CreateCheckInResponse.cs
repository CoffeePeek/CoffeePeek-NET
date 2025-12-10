namespace CoffeePeek.Contract.Response.CoffeeShop;

public class CreateCheckInResponse(Guid checkInId, Guid? reviewId = null)
{
    public Guid CheckInId { get; init; } = checkInId;
    public Guid? ReviewId { get; init; } = reviewId;
}