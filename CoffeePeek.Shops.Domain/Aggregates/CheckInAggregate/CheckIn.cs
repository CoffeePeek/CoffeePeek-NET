using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Entities;
using CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;

namespace CoffeePeek.Shops.Domain.Aggregates.CheckInAggregate;

public sealed partial class CheckIn : AggregateRoot<Guid>
{
    public string? Note { get; private set; }

    public Guid UserId { get; private set; }
    public Guid ShopId { get; private set; }
    public DateTime VisitedAt { get; private set; }
    public Guid? ReviewId { get; private set; }
    
    public Rating Rating { get; private set; }
    
    private readonly List<ShopPhoto> _shopPhotos = [];
    public IReadOnlyCollection<ShopPhoto> ShopPhotos => _shopPhotos.AsReadOnly();
    
    private CheckIn()
    {
        //ef core
    }

    private CheckIn(Guid userId, Guid shopId, DateTime visitedAt)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        ShopId = shopId;
        VisitedAt = visitedAt;
        CreatedAtUtc = DateTime.UtcNow;
    }
}