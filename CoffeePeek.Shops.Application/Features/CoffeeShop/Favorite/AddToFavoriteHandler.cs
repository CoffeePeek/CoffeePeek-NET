using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shops.Application.Services;
using CoffeePeek.Shops.Domain.Entities;
using MediatR;

namespace CoffeePeek.Shops.Application.Handlers.CoffeeShop.Favorite;

public class AddToFavoriteHandler(
    IGenericRepository<FavoriteShop> favoriteShopRepository,
    IGenericRepository<Shop> shopRepository,
    IUnitOfWork unitOfWork,
    IValidationStrategy<AddToFavoriteCommand> validationStrategy,
    IHybridCache hybridCache)
    : IRequestHandler<AddToFavoriteCommand, CreateEntityResponse<Guid>>
{
    public async Task<CreateEntityResponse<Guid>> Handle(AddToFavoriteCommand request, CancellationToken cancellationToken)
    {
        var validationResult = validationStrategy.Validate(request);
        if (!validationResult.IsValid)
        {
            return CreateEntityResponse<Guid>.Error(validationResult.ErrorMessage);
        }

        var shop = await shopRepository.GetByIdAsync(request.CoffeeShopId, cancellationToken);
        if (shop == null)
        {
            return CreateEntityResponse<Guid>.Error("Coffee shop not found");
        }

        var existingFavorite = await favoriteShopRepository.FirstOrDefaultAsync(
            f => f.UserId == request.UserId && f.ShopId == request.CoffeeShopId,
            cancellationToken);

        if (existingFavorite != null)
        {
            return CreateEntityResponse<Guid>.Error("Coffee shop is already in favorites");
        }

        var favoriteShop = new FavoriteShop
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            ShopId = request.CoffeeShopId
        };

        await favoriteShopRepository.AddAsync(favoriteShop, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        await hybridCache.RemoveAsync(CacheKey.Shop.Favorites(request.UserId), cancellationToken);

        return CreateEntityResponse<Guid>.Success(favoriteShop.Id, "Coffee shop added to favorites");
    }
}