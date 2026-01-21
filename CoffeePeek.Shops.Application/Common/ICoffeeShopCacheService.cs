namespace CoffeePeek.Shops.Application.Common;

public interface ICoffeeShopCacheService
{
    Task InvalidateShopCacheAsync(Guid shopId, CancellationToken cancellationToken);
}