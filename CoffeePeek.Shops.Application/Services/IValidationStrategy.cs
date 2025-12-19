namespace CoffeePeek.Shops.Application.Services;

public interface IValidationStrategy<in TEntity>
{
    ValidationResult Validate(TEntity entity);
}