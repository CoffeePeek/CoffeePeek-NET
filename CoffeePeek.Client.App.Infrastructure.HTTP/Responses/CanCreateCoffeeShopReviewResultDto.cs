namespace CoffeePeek.Client.App.Infrastructure.HTTP.Responses;

public sealed class CanCreateCoffeeShopReviewResultDto
{
    public bool CanCreate { get; init; }
    public Guid? ReviewId { get; init; }
}
