using CoffeePeek.Shared.Domain.Entities;
using CoffeePeek.Shared.Kernel.Exceptions;

namespace CoffeePeek.Shops.Domain.Aggregates.UserFavoriteAggregate;

public sealed class UserFavorite : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public Guid CoffeeShopId { get; private set; }
    
    private UserFavorite() { }

    private UserFavorite(Guid userId, Guid coffeeShopId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        CoffeeShopId = coffeeShopId;
    }

    public static UserFavorite Create(Guid userId, Guid coffeeShopId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId cannot be empty");
        
        if (coffeeShopId == Guid.Empty)
            throw new DomainException("CoffeeShopId cannot be empty");

        return new UserFavorite(userId, coffeeShopId);
    }
}