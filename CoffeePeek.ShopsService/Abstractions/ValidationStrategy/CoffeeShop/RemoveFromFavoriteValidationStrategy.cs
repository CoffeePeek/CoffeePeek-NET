using CoffeePeek.BusinessLogic.Abstractions;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.ShopsService.Abstractions;

namespace CoffeePeek.ShopsService.Abstractions.ValidationStrategy.CoffeeShop;

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