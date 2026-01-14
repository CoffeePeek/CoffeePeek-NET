using CoffeePeek.Shared.Validation;
using CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShops;

namespace CoffeePeek.Shops.Infrastructure.ValidationStrategy.CoffeeShop;

public class GetCoffeeShopsValidationStrategy : IValidationStrategy<GetCoffeeShopsQuery>
{
    private const int MinPageNumber = 1;
    private const int MaxPageNumber = 1000;
    private const int MinPageSize = 1;
    private const int MaxPageSize = 100;

    public ValidationResult Validate(GetCoffeeShopsQuery query)
    {
        if (query.CityId == Guid.Empty)
        {
            return ValidationResult.Invalid("CityId is required and cannot be empty");
        }

        return query.PageNumber switch
        {
            < MinPageNumber => ValidationResult.Invalid($"PageNumber must be at least {MinPageNumber}"),
            > MaxPageNumber => ValidationResult.Invalid($"PageNumber cannot exceed {MaxPageNumber}"),
            _ => query.PageSize switch
            {
                < MinPageSize => ValidationResult.Invalid($"PageSize must be at least {MinPageSize}"),
                > MaxPageSize => ValidationResult.Invalid($"PageSize cannot exceed {MaxPageSize}"),
                _ => ValidationResult.Valid
            }
        };
    }
}