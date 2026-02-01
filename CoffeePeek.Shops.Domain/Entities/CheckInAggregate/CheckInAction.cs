using CoffeePeek.Shared.Extensions.Exceptions;

namespace CoffeePeek.Shops.Domain.Entities.CheckInAggregate;

public partial class CheckIn
{
    #region Factory Methods

    public static CheckIn Create(Guid userId, Guid shopId, DateTime visitedAt)
    {
        if (userId == Guid.Empty) throw new DomainException("UserId is required.");
        if (shopId == Guid.Empty) throw new DomainException("ShopId is required.");

        return new CheckIn(userId, shopId, visitedAt);
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