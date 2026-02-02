using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;

namespace CoffeePeek.Shops.Domain.Aggregates.CheckInAggregate;

public partial class CheckIn
{
    #region Factory Methods

    public static Aggregates.CheckInAggregate.CheckIn Create(Guid userId, Guid shopId, DateTime visitedAt)
    {
        if (userId == Guid.Empty) throw new DomainException("UserId is required.");
        if (shopId == Guid.Empty) throw new DomainException("ShopId is required.");

        return new Aggregates.CheckInAggregate.CheckIn(userId, shopId, visitedAt);
    }

    #endregion

    #region Domain Logic

    public void UpdateNote(string? newNote)
    {
        Note = newNote?.Trim();
    }

    public void AddPhotos(IEnumerable<ShopPhoto> photos)
    {
        _shopPhotos.AddRange(photos);
    }

    #endregion
}