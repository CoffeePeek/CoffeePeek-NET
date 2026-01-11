namespace CoffeePeek.Shops.Application.Common;

public interface IShopCacheService
{
    Task InvalidateShopCacheAsync(Guid shopId, CancellationToken cancellationToken);
}