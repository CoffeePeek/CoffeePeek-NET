using CoffeePeek.BusinessLogic.Abstractions;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.ShopsService.Abstractions;

namespace CoffeePeek.ShopsService.Abstractions.ValidationStrategy.CoffeeShop;

public class GetCoffeeShopsValidationStrategy : IValidationStrategy<GetCoffeeShopsCommand>
{
    private const int MinPageNumber = 1;
    private const int MaxPageNumber = 1000;
    private const int MinPageSize = 1;
    private const int MaxPageSize = 100;

    public ValidationResult Validate(GetCoffeeShopsCommand command)
    {
        if (command.CityId == Guid.Empty)
        {
            return ValidationResult.Invalid("CityId is required and cannot be empty");
        }

        if (command.PageNumber < MinPageNumber)
        {
            return ValidationResult.Invalid($"PageNumber must be at least {MinPageNumber}");
        }

        if (command.PageNumber > MaxPageNumber)
        {
            return ValidationResult.Invalid($"PageNumber cannot exceed {MaxPageNumber}");
        }

        if (command.PageSize < MinPageSize)
        {
            return ValidationResult.Invalid($"PageSize must be at least {MinPageSize}");
        }

        if (command.PageSize > MaxPageSize)
        {
            return ValidationResult.Invalid($"PageSize cannot exceed {MaxPageSize}");
        }

        return ValidationResult.Valid;
    }
}

