namespace CoffeePeek.Shared.Validation;

public interface IValidationStrategy<in TEntity>
{
    ValidationResult Validate(TEntity entity);
}