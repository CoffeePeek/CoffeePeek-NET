namespace CoffeePeek.Contract.Events.Shops;

public record ReviewAddedEvent
{
    public Guid UserId { get; init; }
    public Guid ShopId { get; init; }
    public Guid ReviewId { get; init; }
    public DateTime CreatedAt { get; init; }
}