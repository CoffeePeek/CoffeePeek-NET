using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.ShopsService.Abstractions.ValidationStrategy;
using CoffeePeek.ShopsService.Entities;
using MediatR;

namespace CoffeePeek.ShopsService.Handlers.CoffeeShop;

public class AddToFavoriteHandler(
    IGenericRepository<FavoriteShop> favoriteShopRepository,
    IGenericRepository<Shop> shopRepository,
    IUnitOfWork unitOfWork,
    IValidationStrategy<AddToFavoriteCommand> validationStrategy)
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

        return CreateEntityResponse<Guid>.Success(favoriteShop.Id, "Coffee shop added to favorites");
    }
}