using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using CoffeePeek.ShopsService.Abstractions.ValidationStrategy;
using CoffeePeek.ShopsService.Entities;
using MediatR;

namespace CoffeePeek.ShopsService.Handlers.CoffeeShop;

public class RemoveFromFavoriteHandler(
    IGenericRepository<FavoriteShop> favoriteShopRepository,
    IUnitOfWork unitOfWork,
    IValidationStrategy<RemoveFromFavoriteCommand> validationStrategy,
    IRedisService redisService)
    : IRequestHandler<RemoveFromFavoriteCommand, UpdateEntityResponse<Guid>>
{
    public async Task<UpdateEntityResponse<Guid>> Handle(RemoveFromFavoriteCommand request, CancellationToken cancellationToken)
    {
        var validationResult = validationStrategy.Validate(request);
        if (!validationResult.IsValid)
        {
            return UpdateEntityResponse<Guid>.Error(validationResult.ErrorMessage);
        }

        var favoriteShop = await favoriteShopRepository.FirstOrDefaultAsync(
            f => f.UserId == request.UserId && f.ShopId == request.CoffeeShopId,
            cancellationToken);

        if (favoriteShop == null)
        {
            return UpdateEntityResponse<Guid>.Error("Coffee shop is not in favorites");
        }

        var shopId = favoriteShop.ShopId;

        favoriteShopRepository.Remove(favoriteShop);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        await redisService.RemoveAsync(CacheKey.Shop.Favorites(request.UserId));

        return UpdateEntityResponse<Guid>.Success(shopId, "Coffee shop removed from favorites");
    }
}