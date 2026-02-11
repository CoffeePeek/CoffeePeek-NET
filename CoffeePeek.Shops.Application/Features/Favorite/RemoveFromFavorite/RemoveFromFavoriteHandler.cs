using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shared.Validation;
using CoffeePeek.Shops.Domain.Aggregates.UserFavoriteAggregate;
using Wolverine.Attributes;

namespace CoffeePeek.Shops.Application.Features.Favorite.RemoveFromFavorite;

public class RemoveFromFavoriteHandler
{
    [Transactional]
    public async Task<UpdateEntityResponse<Guid>> Handle(RemoveFromFavoriteCommand request,
        IUserFavoriteService favoriteService,
        IValidationStrategy<RemoveFromFavoriteCommand> validationStrategy,
        CancellationToken cancellationToken)
    {
        var validationResult = validationStrategy.Validate(request);
        if (!validationResult.IsValid)
        {
            return UpdateEntityResponse<Guid>.Error(validationResult.ErrorMessage);
        }

        await favoriteService.RemoveFromFavoritesAsync(request.UserId, request.CoffeeShopId,
            cancellationToken);

        return UpdateEntityResponse<Guid>.Success(request.CoffeeShopId, "Coffee shop removed from favorites");
    }
}