using System.ComponentModel.DataAnnotations;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shared.Validation;
using CoffeePeek.Shops.Domain.Aggregates.UserFavoriteAggregate;

namespace CoffeePeek.Shops.Application.Features.Favorite.AddToFavorite;

public class AddToFavoriteHandler
{
    public async Task<CreateEntityResponse<Guid>> Handle(
        AddToFavoriteCommand request,
        IUserFavoriteService userFavoriteService,
        IValidationStrategy<AddToFavoriteCommand> validationStrategy,
        CancellationToken cancellationToken)
    {
        var validationResult = validationStrategy.Validate(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.ErrorMessage);
        }

        var id = await userFavoriteService.AddToFavoritesAsync(request.UserId, request.CoffeeShopId,
            cancellationToken);

        return CreateEntityResponse<Guid>.Success(id, "Coffee shop added to favorites");
    }
}