using CoffeePeek.BusinessLogic.Abstractions;

namespace CoffeePeek.Shops.Abstractions;

public interface IValidationStrategy<in TEntity>
{
    ValidationResult Validate(TEntity entity);
}