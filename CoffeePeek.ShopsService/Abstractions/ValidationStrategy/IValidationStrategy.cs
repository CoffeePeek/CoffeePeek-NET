using CoffeePeek.BusinessLogic.Abstractions;

namespace CoffeePeek.ShopsService.Abstractions.ValidationStrategy;

public interface IValidationStrategy<in TEntity>
{
    ValidationResult Validate(TEntity entity);
}