namespace CoffeePeek.Client.App.Infrastructure.HTTP.Responses;

public sealed class SendCoffeeShopToModerationResponseDto
{
    public Guid ShopId { get; init; }

    public string Status { get; init; } = string.Empty;
}
