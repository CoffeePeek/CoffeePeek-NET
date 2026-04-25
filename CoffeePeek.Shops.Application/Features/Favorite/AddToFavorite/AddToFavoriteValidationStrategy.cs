using CoffeePeek.Shared.Validation;

namespace CoffeePeek.Shops.Application.Features.Favorite.AddToFavorite;

public class AddToFavoriteValidationStrategy : IValidationStrategy<AddToFavoriteCommand>
{
    public ValidationResult Validate(AddToFavoriteCommand command)
    {
        if (command.CoffeeShopId == Guid.Empty)
        {
            return ValidationResult.Invalid("CoffeeShopId is required and cannot be empty");
        }

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (command.UserId == Guid.Empty)
        {
            return ValidationResult.Invalid("UserId is required and cannot be empty");
        }

        return ValidationResult.Valid;
    }
}