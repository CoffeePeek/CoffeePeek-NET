using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Shops.Application;
using CoffeePeek.Shops.Application.Commands.CoffeeShop;
using CoffeePeek.Shops.Application.Services;

namespace CoffeePeek.Shops.Infrastructure.ValidationStrategy.CoffeeShop;

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

        return command.PageNumber switch
        {
            < MinPageNumber => ValidationResult.Invalid($"PageNumber must be at least {MinPageNumber}"),
            > MaxPageNumber => ValidationResult.Invalid($"PageNumber cannot exceed {MaxPageNumber}"),
            _ => command.PageSize switch
            {
                < MinPageSize => ValidationResult.Invalid($"PageSize must be at least {MinPageSize}"),
                > MaxPageSize => ValidationResult.Invalid($"PageSize cannot exceed {MaxPageSize}"),
                _ => ValidationResult.Valid
            }
        };
    }
}