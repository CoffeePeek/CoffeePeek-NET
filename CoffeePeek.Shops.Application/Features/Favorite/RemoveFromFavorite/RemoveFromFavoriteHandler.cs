using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shared.Validation;
using CoffeePeek.Shops.Domain.Aggregates.UserFavoriteAggregate;

namespace CoffeePeek.Shops.Application.Features.Favorite.RemoveFromFavorite;

public class RemoveFromFavoriteHandler
{
    public static async Task<UpdateEntityResponse<Guid>> Handle(RemoveFromFavoriteCommand request,
        IUserFavoriteService favoriteService,
        IValidationStrategy<RemoveFromFavoriteCommand> validationStrategy,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var validationResult = validationStrategy.Validate(request);
        if (!validationResult.IsValid)
        {
            return UpdateEntityResponse<Guid>.Error(validationResult.ErrorMessage);
        }

        await favoriteService.RemoveFromFavoritesAsync(request.UserId, request.CoffeeShopId,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UpdateEntityResponse<Guid>.Success(request.CoffeeShopId, "Coffee shop removed from favorites");
    }
}