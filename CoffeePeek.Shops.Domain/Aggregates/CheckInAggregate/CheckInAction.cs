using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shops.Domain.Entities;

namespace CoffeePeek.Shops.Domain.Aggregates.CheckInAggregate;

public partial class CheckIn
{
    public static CheckIn Create(Guid userId, Guid shopId, DateTime visitedAt)
    {
        if (userId == Guid.Empty) throw new DomainException("UserId is required.");
        if (shopId == Guid.Empty) throw new DomainException("ShopId is required.");

        return new CheckIn(userId, shopId, visitedAt);
    }

    public void UpdateNote(string? newNote)
    {
        Note = newNote?.Trim();
    }

    public void AddPhotos(IEnumerable<ShopPhoto> photos)
    {
        _shopPhotos.AddRange(photos);
    }
}