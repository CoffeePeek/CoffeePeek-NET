using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Shared.Validation;
using CoffeePeek.Shops.Application.Commands.CoffeeShop.Favorite;

namespace CoffeePeek.Shops.Infrastructure.ValidationStrategy.CoffeeShop;

public class RemoveFromFavoriteValidationStrategy : IValidationStrategy<RemoveFromFavoriteCommand>
{
    public ValidationResult Validate(RemoveFromFavoriteCommand command)
    {
        if (command.CoffeeShopId == Guid.Empty)
        {
            return ValidationResult.Invalid("CoffeeShopId is required and cannot be empty");
        }

        if (command.UserId == Guid.Empty)
        {
            return ValidationResult.Invalid("UserId is required and cannot be empty");
        }

        return ValidationResult.Valid;
    }
}