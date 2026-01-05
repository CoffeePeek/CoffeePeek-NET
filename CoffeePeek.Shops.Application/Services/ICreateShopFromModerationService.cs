using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Shops.Application.Services;

public interface ICreateShopFromModerationService
{
    Task CreateShopFromApprovedEventAsync(ShopDto shopDto, Guid creatorId, Guid moderationId, CancellationToken cancellationToken = default);
}
