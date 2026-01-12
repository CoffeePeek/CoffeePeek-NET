using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Validation;
using CoffeePeek.Shops.Application.Commands.CoffeeShop.Favorite;
using CoffeePeek.Shops.Domain.Entities;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.Favorite;

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