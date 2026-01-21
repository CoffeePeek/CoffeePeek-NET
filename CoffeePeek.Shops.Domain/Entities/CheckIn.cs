using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;

namespace CoffeePeek.Shops.Domain.Entities;

public sealed class CheckIn : Entity<Guid>
{
    public string? Note { get; private set; }

    public Guid UserId { get; private set; }
    public Guid? ReviewId { get; private set; }
    public Guid ShopId { get; private set; }

    public CoffeeShop? CoffeeShop { get; private set; }
    public ReviewAggregate.Review? Review { get; private set; }

    private CheckIn()
    {
    }

    private CheckIn(Guid userId, Guid shopId, string? note, Guid? reviewId = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        ShopId = shopId;
        Note = note?.Trim();
        ReviewId = reviewId;
        CreatedAtUtc = DateTime.UtcNow;
    }

    #region Factory Methods

    public static CheckIn Create(Guid userId, Guid shopId, string? note = null)
    {
        if (userId == Guid.Empty) throw new DomainException("UserId is required.");
        if (shopId == Guid.Empty) throw new DomainException("ShopId is required.");

        return new CheckIn(userId, shopId, note);
    }

    public static CheckIn CreateWithReview(Guid userId, Guid shopId, Guid reviewId, string? note = null)
    {
        if (reviewId == Guid.Empty) throw new DomainException("ReviewId cannot be empty.");

        return new CheckIn(userId, shopId, note, reviewId);
    }

    #endregion

    #region Domain Logic

    public void UpdateNote(string? newNote)
    {
        Note = newNote?.Trim();
    }

    public void LinkReview(Guid reviewId)
    {
        if (ReviewId.HasValue && ReviewId.Value != reviewId)
            throw new DomainException("Check-in is already linked to another review.");

        ReviewId = reviewId;
    }

    #endregion
}