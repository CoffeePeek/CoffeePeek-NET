using CoffeePeek.AuthService.Models;

namespace CoffeePeek.AuthService.Services.Validation;

public interface IValidationStrategy<in TEntity>
{
    ValidationResult Validate(TEntity entity);
}