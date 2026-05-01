namespace CoffeePeek.Client.App.Infrastructure.HTTP.Responses;

public sealed class CreateCoffeeShopReviewResultDto
{
    public Guid CheckInId { get; init; }
    public Guid? ReviewId { get; init; }
}
